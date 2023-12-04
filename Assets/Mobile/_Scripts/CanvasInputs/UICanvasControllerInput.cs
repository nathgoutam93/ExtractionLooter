using UnityEngine;

namespace StarterAssets
{
  public class UICanvasControllerInput : MonoBehaviour
  {
    public PlayerInputs input;

    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
      input.MoveInput(virtualMoveDirection);
    }

    public void VirtualLookInput(Vector2 virtualLookDirection)
    {

      input.LookInput(virtualLookDirection);
    }

    public void VirtualJumpInput(bool virtualJumpState)
    {
      input.OnJumpEvent?.Invoke();
    }

    public void VirtualSprintInput(bool virtualSprintState)
    {
      input.OnSprintEvent?.Invoke(virtualSprintState); //.SprintInput(virtualSprintState);
    }
  }
}
