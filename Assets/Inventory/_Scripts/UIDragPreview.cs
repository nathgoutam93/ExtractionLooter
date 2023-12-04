using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIDragPreview : MonoBehaviour
{
  [SerializeField] private Image icon;
  [SerializeField] private TextMeshProUGUI itemName;
  [SerializeField] private TextMeshProUGUI amount;

  private ItemSlot itemSlot;
  public void Initialize(ItemSlot itemSlot)
  {
    this.itemSlot = itemSlot;

    itemName.text = itemSlot.item.name.ToString();
    amount.text = itemSlot.amount.ToString();
  }
}
