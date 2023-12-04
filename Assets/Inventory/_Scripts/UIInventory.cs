using UnityEngine;

public class UIInventory : MonoBehaviour
{
  public Inventory inventory;
  public GameObject itemSlotPrefab;
  public GameObject dragPreviewPrefeb;
  public Transform itemSlotContainer;

  public UIDropSlot dropSlot;

  private CanvasGroup canvasGroup;
  private ItemSlot draggedItemSlot;
  private GameObject draggedItem;

  private void Start()
  {
    inventory.OnItemsChanged.AddListener(UpdateUI);

    canvasGroup = GetComponent<CanvasGroup>();

    UpdateUI();
  }

  public void UpdateUI()
  {
    foreach (Transform child in itemSlotContainer)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < inventory.itemSlots.Count; i++)
    {
      ItemSlot itemSlot = inventory.itemSlots[i];
      GameObject newItemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
      newItemSlot.GetComponent<UIItemSlot>().Initialize(itemSlot, this, i);
    }
  }

  public void StartDrag(ItemSlot itemSlot)
  {
    if (itemSlot.amount == 0) return;

    draggedItemSlot = itemSlot;
    draggedItem = Instantiate(dragPreviewPrefeb, transform);
    draggedItem.GetComponent<UIDragPreview>().Initialize(itemSlot);
  }

  public void OnDrag(Vector2 pointerPosition)
  {

    dropSlot.gameObject.SetActive(true);

    if (draggedItem != null)
    {
      draggedItem.transform.position = pointerPosition;
    }
  }

  public void EndDrag()
  {
    dropSlot.gameObject.SetActive(false);

    if (draggedItem != null)
    {
      Destroy(draggedItem);
    }
  }

  public void DropItem(int index)
  {
    ItemSlot itemSlot = inventory.itemSlots[index];

    if (itemSlot != null)
    {
      inventory.itemSlots.Remove(itemSlot);
      var droppedItem = Instantiate(itemSlot.item.Prefeb, inventory.gameObject.transform.position + (inventory.gameObject.transform.up * 2) + (inventory.gameObject.transform.forward * 2), Quaternion.identity);
      droppedItem.SetAmount(itemSlot.amount);
    }

    if (draggedItem != null)
    {
      Destroy(draggedItem);
    }

    dropSlot.gameObject.SetActive(false);

    UpdateUI();
  }

  public void ToggleInventory()
  {
    if (gameObject.activeSelf)
    {
      Hide();
    }
    else
    {
      Show();
    }
  }

  private void Hide()
  {
    gameObject.SetActive(false);
  }

  private void Show()
  {
    gameObject.SetActive(true);
  }

}
