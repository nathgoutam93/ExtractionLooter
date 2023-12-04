using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{

  [SerializeField] private float topClamp = 70.0f;
  [SerializeField] private float bottomClamp = -30.0f;
  [SerializeField] private PlayerInputs input;

  private float cinemachineTargetYaw;
  private float cinemachineTargetPitch;
  private float threshold = 0.1f;

  void Start()
  {
    cinemachineTargetYaw = transform.rotation.eulerAngles.y;
    cinemachineTargetPitch = transform.rotation.eulerAngles.x;
  }

  void LateUpdate()
  {
    PerformLook();
  }

  private void PerformLook()
  {
    // if there is an input and camera position is not fixed
    if (input.Look.sqrMagnitude >= threshold)
    {
      cinemachineTargetYaw += input.Look.x;
      cinemachineTargetPitch += input.Look.y;
    }

    // clamp our rotations so our values are limited 360 degrees
    cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
    cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

    // Cinemachine will follow this target
    transform.rotation = Quaternion.Euler(cinemachineTargetPitch, cinemachineTargetYaw, 0.0f);
  }

  private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
  {
    if (lfAngle < -360f) lfAngle += 360f;
    if (lfAngle > 360f) lfAngle -= 360f;
    return Mathf.Clamp(lfAngle, lfMin, lfMax);
  }
}
