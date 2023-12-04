using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Assets.Player.Network
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] NetworkMovement m_PlayerMovement;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private PlayerInputs input;

        [SerializeField] private Transform m_CamSocket;
        [SerializeField] private Camera m_MainCamera;
        [SerializeField] private CinemachineVirtualCamera m_VirutualCam;

        public override void OnNetworkSpawn()
        {
            if(IsLocalPlayer)
            {
                m_VirutualCam.Priority = 1;
                EnableInput();
            }
            else
            {
                m_VirutualCam.Priority = 0;
                m_MainCamera.TryGetComponent<AudioListener>(out AudioListener listener);
                listener.enabled = false;
            }
        }

        private void EnableInput()
        {
            playerInput.enabled = true;
            input.OnJumpEvent.AddListener(m_PlayerMovement.ProcessLocalPlayerJump);
            //input.OnSprintEvent.AddListener((bool value) => { _isSprinting = value; });
        }

        private void Update()
        {
            if(IsClient && IsLocalPlayer)
            {
                m_PlayerMovement.ProcessLocalPlayerMovement(input.Move, m_CamSocket.eulerAngles.y);
            }
            else
            {
                m_PlayerMovement.ProcessSimulatedPlayerMovement();
            }
        }
    }
}