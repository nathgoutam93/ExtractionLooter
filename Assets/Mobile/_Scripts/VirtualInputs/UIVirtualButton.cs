using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIVirtualButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
  [System.Serializable]
  public class BoolEvent : UnityEvent<bool> { }
  [System.Serializable]
  public class Event : UnityEvent { }

  [Header("Output")]
  public BoolEvent buttonHoverStateOutputEvent;
  public BoolEvent buttonStateOutputEvent;
  public Event buttonClickOutputEvent;

  public void OnPointerDown(PointerEventData eventData)
  {
    OutputButtonStateValue(true);
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    OutputButtonStateValue(false);
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    buttonHoverStateOutputEvent?.Invoke(true);
  }

  public void OnPointerExit(PointerEventData eventData)
  {

    buttonHoverStateOutputEvent?.Invoke(false);
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    OutputButtonClickEvent();
  }

  void OutputButtonStateValue(bool buttonState)
  {
    buttonStateOutputEvent?.Invoke(buttonState);
  }

  void OutputButtonClickEvent()
  {
    buttonClickOutputEvent?.Invoke();
  }

}
