using Cinemachine;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Player.Network
{
    public class NetworkMovement : NetworkBehaviour
    {

        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_TurnSpeed;
        [SerializeField] private float m_SpeedChangeRate;
        [SerializeField] private float m_JumpHeight;
        [SerializeField] private float k_Gravity = -9.61f;

        [SerializeField] private CharacterController m_Controller;

        private float m_VerticalVelocity;
        private float m_RotationVelocity;
        private float m_Speed;
        
        private bool m_IsSprinting;

        private int m_Tick = 0;
        private float m_TickDeltaTime = 0f;
        private const float k_TickRate = 1f / 60f;

        private const int k_BufferSize = 1024;
        private InputState[] m_InputStates = new InputState[k_BufferSize];
        private TransformState[] m_TransformStates= new TransformState[k_BufferSize];


        public NetworkVariable<TransformState> ServerTransformState = new NetworkVariable<TransformState>();

        public TransformState _previousTransformState;
        
        private void OnEnable()
        {
            ServerTransformState.OnValueChanged += OnServerStateChanged;
        }

        private void OnServerStateChanged(TransformState previousValue, TransformState newValue)
        {
            _previousTransformState = previousValue;
        }

        public void ProcessLocalPlayerJump()
        {
            if (!IsServer)
            {
                JumpServerRpc();
                PerformJump();
            }
            else
            {
                PerformJump();
            }
        }

        [ServerRpc]
        private void JumpServerRpc()
        {
            PerformJump();
        }

        private void PerformJump()
        {
            if (!m_Controller.isGrounded) return;

            m_VerticalVelocity += Mathf.Sqrt(m_JumpHeight * -2.0f * k_Gravity);

            /*if (hasAnimator)
            {
                animator.SetBool(animIDJump, true);
            }*/
        }

        public void ProcessLocalPlayerMovement(Vector2 moveInput, float lookAt)
        {
            m_TickDeltaTime += Time.deltaTime;
            if(m_TickDeltaTime > k_TickRate)
            {
                int bufferIdx = m_Tick % k_BufferSize;

                if(!IsServer)
                {
                    MovePlayerServerRpc(m_Tick, moveInput, lookAt);
                    MovePlayer(moveInput, lookAt);
                }
                else
                {
                    MovePlayer(moveInput, lookAt);
                }

                InputState inputState = new InputState()
                {
                    Tick = m_Tick,
                    movementInput = moveInput,
                    lookInput = Vector2.zero
                };

                TransformState transformState = new TransformState()
                {
                    Tick = m_Tick,
                    Position = transform.position,
                    Rotation = transform.rotation,
                    HasStartedMoving = true
                };

                if(IsServer)
                {
                    _previousTransformState = ServerTransformState.Value;
                    ServerTransformState.Value = transformState;
                }

                m_InputStates[bufferIdx] = inputState;
                m_TransformStates[bufferIdx] = transformState;

                m_TickDeltaTime -= k_TickRate;
                m_Tick++;
            }
        }

        public void ProcessSimulatedPlayerMovement()
        {
            m_TickDeltaTime += Time.deltaTime;
            if(m_TickDeltaTime > k_TickRate)
            {
                if(ServerTransformState.Value.HasStartedMoving)
                {
                    transform.position = ServerTransformState.Value.Position;
                    transform.rotation = ServerTransformState.Value.Rotation;
                }

                m_TickDeltaTime -= k_TickRate;
                m_Tick++;
            }
        }

        [ServerRpc]
        private void MovePlayerServerRpc(int tick, Vector2 moveInput, float lookAt)
        {
            MovePlayer(moveInput, lookAt);

            TransformState state = new TransformState()
            {
                Tick = tick,
                Position = transform.position,
                Rotation = transform.rotation,
                HasStartedMoving = true
            };

            _previousTransformState = ServerTransformState.Value;
            ServerTransformState.Value = state;
        }

        private void GroundCheck()
        {
            var groundedPlayer = m_Controller.isGrounded;

            if (groundedPlayer && m_VerticalVelocity < 0)
            {
                m_VerticalVelocity = 0f;

                /*if (hasAnimator)
                {
                    animator.SetBool(animIDJump, false);
                    animator.SetBool(animIDFreeFall, false);
                    animator.SetBool(animIDGrounded, true);
                }*/
            }
            else if (m_VerticalVelocity < 0.0f)
            {
                /*if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                    animator.SetBool(animIDFreeFall, true);
                    animator.SetBool(animIDGrounded, false);
                }*/
            }

            m_VerticalVelocity += k_Gravity * k_TickRate;
        }


        private void MovePlayer(Vector2 moveInput, float lookAt)
        {
            GroundCheck();

            float targetSpeed = m_IsSprinting ? m_SprintSpeed : m_MoveSpeed;

            if (moveInput == Vector2.zero)
            {
                targetSpeed = 0.0f;
            }

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(m_Controller.velocity.x, 0.0f, m_Controller.velocity.z).magnitude;

            float speedOffset = 0.1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                m_Speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                    k_TickRate * m_SpeedChangeRate);

                // round speed to 3 decimal places
                m_Speed = Mathf.Round(m_Speed * 1000f) / 1000f;
            }
            else
            {
                m_Speed = targetSpeed;
            }

            // animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            // if (animationBlend < 0.01f) animationBlend = 0f;

            //animationBlendX = Mathf.Lerp(animationBlendX, moveInput.normalized.x, Time.deltaTime * speedChangeRate);
            //animationBlendY = Mathf.Lerp(animationBlendY, moveInput.normalized.y, Time.deltaTime * speedChangeRate);

            Vector3 inputDirection = new Vector3(moveInput.x, 0.0f, moveInput.y).normalized;

            float _targetRotation = lookAt; //Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg + cameraYaw;

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * inputDirection; // Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref m_RotationVelocity, m_TurnSpeed);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            m_Controller.Move(targetDirection.normalized * m_Speed * k_TickRate + new Vector3(0.0f, m_VerticalVelocity, 0.0f) * k_TickRate);

            /*if (hasAnimator)
            {
                // animator.SetFloat(animIDSpeed, animationBlend);
                // animator.SetFloat(animIDMotionSpeed, 1.0f);
                animator.SetFloat(animIDVeloX, animationBlendX);
                animator.SetFloat(animIDVeloY, animationBlendY);
                animator.SetBool(animIDSprinting, isSprinting);
            }*/
        }
    }

}