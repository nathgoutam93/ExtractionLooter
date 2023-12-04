using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float sprintSpeed = 5.0f;
    [SerializeField] private float speedChangeRate = 10.0f;
    [SerializeField] private float rotationSmoothTime = 0.12f;
    [SerializeField] private float JumpHeight = 1.2f;
    [SerializeField] private float Gravity = -15.0f;

    [SerializeField] private PlayerInputs input;
    [SerializeField] private AudioClip FootStepAudioClip;
    [SerializeField] private AudioClip LandAudioClip;
    [SerializeField] private float FootstepAudioVolume;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;
    [SerializeField] private PlayerInput playerInput;
    private CharacterController controller;

    private float _speed;
    private float rotationVelocity;
    private float verticalVelocity;
    private bool _isSprinting;

    public bool IsSprinting => _isSprinting;

    public void SetSprinting(bool value)
    {
        _isSprinting = value;
    }

    public void ToggleSprintState()
    {
        _isSprinting = !_isSprinting;

        if (_isSprinting)
        {
            input.MoveInput(Vector2.up);
        }
        else
        {
            input.MoveInput(Vector2.zero);
        }
    }

    // animation IDs
    private Animator animator;
    private bool hasAnimator;
    // private float animationBlend;
    // private int animIDSpeed;
    // private int animIDMotionSpeed;
    private float animationBlendX;
    private float animationBlendY;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDVeloX;
    private int animIDVeloY;
    private int animIDSprinting;


    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            cinemachineCamera.Priority = 1;
            EnableInput();
        }
        else
        {
            cinemachineCamera.Priority = 0;
            mainCamera.TryGetComponent<AudioListener>(out AudioListener listener);
            listener.enabled = false;
        }
    }

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        AssignAnimationIDs();
    }

    private void EnableInput()
    {
        playerInput.enabled = true;
        input.OnJumpEvent.AddListener(IsServer? PerformJump : JumpServerRpc);
        input.OnSprintEvent.AddListener((bool value) => { _isSprinting = value; });
    }

    private void DisableInput()
    {
        playerInput.enabled = false;
        input.OnJumpEvent.RemoveAllListeners();
        input.OnSprintEvent.RemoveAllListeners();
    }

    private void Update()
    {
        hasAnimator = TryGetComponent(out animator);


        if (IsServer && IsLocalPlayer)
        {
            GroundCheck();
            PerformMove(_isSprinting, input.Move, mainCamera.transform.eulerAngles.y);
        }
        else if (IsLocalPlayer)
        {
            MoveServerRpc(_isSprinting, input.Move, mainCamera.transform.eulerAngles.y);
        }

    }

    [ServerRpc]
    private void MoveServerRpc(bool isSprinting, Vector2 moveInput, float look)
    {
        GroundCheck();
        PerformMove(_isSprinting, moveInput, look);
    }

    private void GroundCheck()
    {
        var groundedPlayer = controller.isGrounded;

        Debug.Log(groundedPlayer);
        Debug.Log(verticalVelocity);

        if (groundedPlayer && verticalVelocity < -0.2f)
        {
            verticalVelocity = 0f;

            if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDGrounded, true);
                //animator.SetBool(animIDFreeFall, false);
            }
        }
        else if (verticalVelocity < -0.2f)
        {
            if (hasAnimator)
            {
                animator.SetBool(animIDJump, true);
                animator.SetBool(animIDGrounded, false);
                //animator.SetBool(animIDFreeFall, true);
            }
        }

        verticalVelocity += Gravity * Time.deltaTime;
    }

  private void PerformMove(bool isSprinting, Vector2 moveInput, float cameraYaw)
  {
    float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

    if (moveInput == Vector2.zero)
    {
      targetSpeed = 0.0f;
    }

    // a reference to the players current horizontal velocity
    float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

    float speedOffset = 0.1f;

    // accelerate or decelerate to target speed
    if (currentHorizontalSpeed < targetSpeed - speedOffset ||
        currentHorizontalSpeed > targetSpeed + speedOffset)
    {
      // creates curved result rather than a linear one giving a more organic speed change
      // note T in Lerp is clamped, so we don't need to clamp our speed
      _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
          Time.deltaTime * speedChangeRate);

      // round speed to 3 decimal places
      _speed = Mathf.Round(_speed * 1000f) / 1000f;
    }
    else
    {
      _speed = targetSpeed;
    }

    // animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
    // if (animationBlend < 0.01f) animationBlend = 0f;

    animationBlendX = Mathf.Lerp(animationBlendX, moveInput.normalized.x, Time.deltaTime * speedChangeRate);
    animationBlendY = Mathf.Lerp(animationBlendY, moveInput.normalized.y, Time.deltaTime * speedChangeRate);

    Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

    float _targetRotation = cameraYaw; //Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg + cameraYaw;

    Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * inputDirection; // Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref rotationVelocity, rotationSmoothTime);

    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

    if (hasAnimator)
    {
      // animator.SetFloat(animIDSpeed, animationBlend);
      // animator.SetFloat(animIDMotionSpeed, 1.0f);
      animator.SetFloat(animIDVeloX, animationBlendX);
      animator.SetFloat(animIDVeloY, animationBlendY);
      animator.SetBool(animIDSprinting, isSprinting);
    }

  }

    [ServerRpc]
    private void JumpServerRpc()
    {
        PerformJump();
    }

  private void PerformJump()
  {

    if (!controller.isGrounded) return;

    verticalVelocity += Mathf.Sqrt(JumpHeight * -2.0f * Gravity);

    if (hasAnimator)
    {
      animator.SetBool(animIDJump, true);
    }
  }

  private void AssignAnimationIDs()
  {
    // animIDSpeed = Animator.StringToHash("Speed");
    // animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    animIDVeloX = Animator.StringToHash("velocityX");
    animIDVeloY = Animator.StringToHash("velocityY");
    animIDGrounded = Animator.StringToHash("Grounded");
    animIDJump = Animator.StringToHash("Jump");
    animIDFreeFall = Animator.StringToHash("FreeFall");
    animIDSprinting = Animator.StringToHash("Sprinting");
  }

  private void OnFootstep(AnimationEvent animationEvent)
  {
    if (animationEvent.animatorClipInfo.weight > 0.5f)
    {
      AudioSource.PlayClipAtPoint(FootStepAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
    }
  }

  private void OnLand(AnimationEvent animationEvent)
  {
    if (animationEvent.animatorClipInfo.weight > 0.5f)
    {
      AudioSource.PlayClipAtPoint(LandAudioClip, transform.TransformPoint(controller.center), FootstepAudioVolume);
    }
  }

}
