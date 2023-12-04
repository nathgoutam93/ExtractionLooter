using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualSwipeZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
  [System.Serializable]
  public class Event : UnityEvent<Vector2> { }

  [Header("Settings")]
  public float Sensitivity = 10;
  public float _threshold = 1f;
  public bool invertXOutputValue;
  public bool invertYOutputValue;

  private Vector2 startPosition;
  private Vector2 endPosition;
  private Vector2 lastSwipeDirection = Vector2.zero;

  private bool _swiping;

  [Header("Output")]
  public Event swipeZoneOutputEvent;

  private void Update()
  {
    if (_swiping)
    {
      // Calculate the swipe direction and distance
      Vector2 swipeDirection = (endPosition - startPosition).normalized;
      swipeDirection = ApplyInversionFilter(swipeDirection);

      // Check if the swipe was long enough to count as a swipe
      float swipeDistance = Vector2.Distance(swipeDirection, lastSwipeDirection);
      if (swipeDistance > _threshold)
      {
        OutputPointerEventValue(swipeDirection * Sensitivity);
        lastSwipeDirection = swipeDirection;
      }

      startPosition = endPosition;
    }
  }

  public void OnPointerDown(PointerEventData eventData)
  {
    _swiping = true;
    startPosition = eventData.position;
    endPosition = eventData.position;
  }

  public void OnPointerMove(PointerEventData eventData)
  {
    endPosition = eventData.position;
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    OutputPointerEventValue(Vector2.zero);
    _swiping = false;
  }

  void OutputPointerEventValue(Vector2 pointerPosition)
  {
    swipeZoneOutputEvent?.Invoke(pointerPosition);
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
