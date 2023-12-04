using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerInputs : MonoBehaviour
{
  public UnityEvent OnJumpEvent;
  public UnityEvent<bool> OnSprintEvent;
  public UnityEvent OnInteractEvent;

  [SerializeField] private InputActionAsset playerInputAction;
  
    private Vector2 move;
    private Vector2 look;
  // private bool sprint;

  public Vector2 Move { get { return move; } }
  public Vector2 Look { get { return look; } }

  // public bool IsSprinting { get { return sprint; } }

  private InputAction moveAction;
  private InputAction lookAction;
  private InputAction sprintAction;
  private InputAction jumpAction;

  public void MoveInput(Vector2 InputValue)
  {
    move = InputValue;
  }

  public void LookInput(Vector2 InputValue)
  {
    look = InputValue;
  }

  // public void SprintInput(bool InputValue)
  // {
  //   sprint = InputValue;
  // }

  public void OnMove(InputValue value)
  {
    MoveInput(value.Get<Vector2>());
  }

  public void OnLook(InputValue value)
  {
    LookInput(value.Get<Vector2>());
  }

  public void OnJump(InputValue value)
  {
    OnJumpEvent?.Invoke();
  }

  public void OnInteract(InputValue value)
  {
    OnInteractEvent?.Invoke();
  }

  public void OnSprint(InputValue value)
  {
    OnSprintEvent?.Invoke(value.isPressed);
    // SprintInput(value.isPressed);
  }
}
