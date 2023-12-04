using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIItemSlot : MonoBehaviour, IPointerClickHandler //, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{

  [SerializeField] private Image icon;
  [SerializeField] private TextMeshProUGUI itemName;
  [SerializeField] private TextMeshProUGUI amount;
  [SerializeField] private Button useBtn;
  // [SerializeField] private float dragThreshold = 20f;
  private Vector2 initialPointerPosition;

  private ItemSlot itemSlot;
  private UIInventory inventoryUI;
  private int index;

  private bool dragging;

  public int Index => index;

  public void Initialize(ItemSlot itemSlot, UIInventory inventoryUI, int index)
  {
    this.itemSlot = itemSlot;
    this.inventoryUI = inventoryUI;
    this.index = index;

    itemName.text = itemSlot.item.name.ToString();
    amount.text = itemSlot.amount.ToString();
  }

  private void Consume()
  {
    inventoryUI.inventory.ConsumeItem(itemSlot);
  }

  // public void OnPointerDown(PointerEventData eventData)
  // {
  //   initialPointerPosition = eventData.position;
  // }

  // public void OnPointerMove(PointerEventData eventData)
  // {
  //   if (!dragging)
  //   {
  //     float horizontalDifference = Mathf.Abs(eventData.position.x - initialPointerPosition.x);
  //     float verticalDifference = Mathf.Abs(eventData.position.y - initialPointerPosition.y);

  //     if (horizontalDifference > verticalDifference)
  //     {
  //       dragging = true;
  //       inventoryUI.StartDrag(itemSlot);
  //     }
  //     else
  //     {
  //       return;
  //     }
  //   }

  //   inventoryUI.OnDrag(eventData.position);
  // }

  // public void OnPointerUp(PointerEventData eventData)
  // {
  //   if (dragging)
  //   {
  //     inventoryUI.EndDrag();
  //     dragging = false;
  //   }
  // }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (itemSlot.item.isConsumable)
    {
      useBtn.gameObject.SetActive(true);
      useBtn.onClick.AddListener(Consume);
    }
  }

}
