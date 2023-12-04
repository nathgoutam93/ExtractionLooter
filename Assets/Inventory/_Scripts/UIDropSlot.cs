using UnityEngine;
using UnityEngine.EventSystems;

public class UIDropSlot : MonoBehaviour, IDropHandler
{

  [SerializeField] private UIInventory inventoryUI;

  public void OnDrop(PointerEventData eventData)
  {
    var slotUI = eventData.pointerDrag.GetComponent<UIItemSlot>();
    if (slotUI != null)
    {
      int index = slotUI.Index;
      inventoryUI.DropItem(index);
    }
  }
}
