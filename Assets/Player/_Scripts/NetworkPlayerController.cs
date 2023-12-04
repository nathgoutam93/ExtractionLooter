using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Cinemachine;
using Unity.Netcode.Transports.UTP;

public class NetworkPlayerController : NetworkBehaviour
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

  // animation IDs
  private Animator animator;
  private bool hasAnimator;
  private float animationBlend;
  private float animationBlendX;
  private float animationBlendY;
  private int animIDSpeed;
  private int animIDGrounded;
  private int animIDJump;
  private int animIDFreeFall;
  private int animIDMotionSpeed;

  public override void OnNetworkSpawn()
  {
    base.OnNetworkSpawn();

    UnityTransport.ConnectionAddressData connectionData = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData;
    Debug.Log("add: " + connectionData.Address + " port: " + connectionData.Port);

    if (IsLocalPlayer)
    {
      playerInput.enabled = true;
      mainCamera.gameObject.SetActive(true);
      cinemachineCamera.gameObject.SetActive(true);
    }
  }

  private void Start()
  {
    controller = GetComponentInChildren<CharacterController>();

    if (IsLocalPlayer)
    {
      input.OnJumpEvent.AddListener(PerformJump);
      input.OnSprintEvent.AddListener((bool value) => { _isSprinting = value; });
    }

    AssignAnimationIDs();
  }

  private void Update()
  {
    hasAnimator = TryGetComponent(out animator);

    if (IsLocalPlayer)
    {
      GroundCheckServerRpc();
      PerformMove();
    }
  }

  private void PerformMove()
  {
    MoveServerRpc(_isSprinting, input.Move, mainCamera.transform.eulerAngles.y);
  }

  private void PerformJump()
  {
    JumpServerRpc();
  }

  [ServerRpc]
  private void GroundCheckServerRpc()
  {
    var groundedPlayer = controller.isGrounded;

    if (groundedPlayer && verticalVelocity < 0)
    {
      verticalVelocity = 0f;

      if (hasAnimator)
      {
        animator.SetBool(animIDJump, false);
        animator.SetBool(animIDFreeFall, false);
        animator.SetBool(animIDGrounded, true);
      }
    }
    else if (verticalVelocity < 0.0f)
    {
      if (hasAnimator)
      {
        animator.SetBool(animIDJump, true);
        animator.SetBool(animIDFreeFall, true);
        animator.SetBool(animIDGrounded, false);
      }
    }

    verticalVelocity += Gravity * Time.deltaTime;
  }

  [ServerRpc]
  private void MoveServerRpc(bool isSprinting, Vector2 moveInput, float cameraYaw)
  {
    // set target speed based on move speed, sprint speed and if sprint is pressed
    float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

    // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

    // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
    // if there is no input, set the target speed to 0
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

    // normalise input direction
    Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

    float _targetRotation = 0.0f;

    // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
    // if there is a move input rotate player when the player is moving
    if (moveInput != Vector2.zero)
    {
      _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cameraYaw;
      float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref rotationVelocity, rotationSmoothTime);

      // rotate to face input direction relative to camera position
      transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    }

    Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

    // move the player
    controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

    if (hasAnimator)
    {
      // animator.SetFloat(animIDSpeed, animationBlend);
      // animator.SetFloat(animIDMotionSpeed, 1.0f);

      animator.SetFloat("velocityX", animationBlendX);
      animator.SetFloat("velocityY", animationBlendY);
      animator.SetBool("Sprinting", isSprinting);
    }
  }

  [ServerRpc]
  private void JumpServerRpc()
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
    animIDSpeed = Animator.StringToHash("Speed");
    animIDGrounded = Animator.StringToHash("Grounded");
    animIDJump = Animator.StringToHash("Jump");
    animIDFreeFall = Animator.StringToHash("FreeFall");
    animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
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
