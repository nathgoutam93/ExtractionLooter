using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualJoystickWithSprintLock : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
  [System.Serializable]
  public class Event : UnityEvent<Vector2> { }
  [System.Serializable]
  public class BoolEvent : UnityEvent<bool> { }

  [Header("Rect References")]
  public RectTransform containerRect;
  public RectTransform joyStickRect;
  public RectTransform handleRect;
  public RectTransform sprintRect;

  [Header("Settings")]
  public float joystickRange = 50f;
  public float magnitudeMultiplier = 1f;
  public float sprintThreshold = 2f;
  public bool sprintLocked = false;
  public bool invertXOutputValue;
  public bool invertYOutputValue;

  [Header("Output")]
  public Event joystickOutputEvent;
  public BoolEvent joystickSprintOutputEvent;

  void Start()
  {
    SetupHandle();
  }

  private void SetupHandle()
  {
    if (joyStickRect)
    {
      UpdateJoyStickRectPosition(Vector2.zero);
    }

    if (handleRect)
    {
      UpdateHandleRectPosition(Vector2.zero);
    }
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    // Get the touch position in the local space of the container rectangle
    RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out Vector2 position);

    // Calculate the position of the handle relative to the touch position
    // Vector2 handlePosition = position - joyStickRect.anchoredPosition;

    // Set the handle position to the touch position
    joyStickRect.anchoredPosition = position;

    SetObjectActiveState(joyStickRect.gameObject, true);

    OnDrag(eventData);
  }


  public void OnDrag(PointerEventData eventData)
  {

    RectTransformUtility.ScreenPointToLocalPointInRectangle(joyStickRect, eventData.position, eventData.pressEventCamera, out Vector2 position);

    position = ApplySizeDelta(position);

    Vector2 clampedPosition = ClampValuesToMagnitude(position);

    Vector2 outputPosition = ApplyInversionFilter(position);

    if (position.magnitude < 0.1f) // If the joystick is not being moved
    {
      if (sprintLocked) // If the player is currently sprinting
      {
        outputPosition.y = 1f; // Set the joystick input to move the player forward
      }
    }
    else
    {
      if (outputPosition.y < 1f)
      {
        SetObjectActiveState(sprintRect.gameObject, false);
      }
      else
      {
        SetObjectActiveState(sprintRect.gameObject, true);
      }

      if (outputPosition.y > sprintThreshold && outputPosition.x < 0.2f && outputPosition.x > -0.2f) // If the joystick is moved forward beyond a certain threshold
      {
        if (!sprintLocked)
        {
          sprintLocked = true; // Set the sprint flag
          OutputSprintEventValue(sprintLocked);
        }
      }
      else if (sprintLocked)
      {
        sprintLocked = false; // Set the sprint flag
        OutputSprintEventValue(sprintLocked);
      }
    }

    OutputPointerEventValue(clampedPosition * magnitudeMultiplier);

    if (handleRect)
    {
      UpdateHandleRectPosition(clampedPosition * joystickRange);
    }

  }

  public void OnPointerUp(PointerEventData eventData)
  {
    if (!sprintLocked)
    {
      OutputPointerEventValue(Vector2.zero);
    }

    if (handleRect)
    {
      UpdateHandleRectPosition(Vector2.zero);
    }

    if (joyStickRect)
    {
      UpdateJoyStickRectPosition(Vector2.zero);

      SetObjectActiveState(joyStickRect.gameObject, false);
    }
  }

  private void OutputPointerEventValue(Vector2 position)
  {
    joystickOutputEvent?.Invoke(position);
  }

  private void OutputSprintEventValue(bool value)
  {
    joystickSprintOutputEvent?.Invoke(value);
  }

  private void UpdateSprintRectPosition(Vector2 newPosition)
  {
    sprintRect.anchoredPosition = newPosition;
  }

  private void UpdateHandleRectPosition(Vector2 newPosition)
  {
    handleRect.anchoredPosition = newPosition;
  }

  private void UpdateJoyStickRectPosition(Vector2 newPosition)
  {
    joyStickRect.anchoredPosition = newPosition;
  }

  void SetObjectActiveState(GameObject targetObject, bool newState)
  {
    targetObject.SetActive(newState);
  }

  Vector2 ApplySizeDelta(Vector2 position)
  {
    float x = (position.x / joyStickRect.sizeDelta.x) * 2.5f;
    float y = (position.y / joyStickRect.sizeDelta.y) * 2.5f;
    return new Vector2(x, y);
  }

  Vector2 ClampValuesToMagnitude(Vector2 position)
  {
    return Vector2.ClampMagnitude(position, 1);
  }

  Vector2 ApplyInversionFilter(Vector2 position)
  {
    if (invertXOutputValue)
    {
      position.x = InvertValue(position.x);
    }

    if (invertYOutputValue)
    {
      position.y = InvertValue(position.y);
    }

    return position;
  }

  float InvertValue(float value)
  {
    return -value;
  }
}