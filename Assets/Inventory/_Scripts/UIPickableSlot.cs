using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIPickableSlot : MonoBehaviour, IPointerClickHandler
{

  [SerializeField] private UIInteraction interaction;
  [SerializeField] private Image icon;
  [SerializeField] private TextMeshProUGUI itemName;
  [SerializeField] private TextMeshProUGUI amount;
  private PickableItem pickableItem;

  public void Initialize(PickableItem pickableItem, UIInteraction interaction)
  {
    this.pickableItem = pickableItem;
    this.interaction = interaction;

    itemName.text = pickableItem.GetItemSlot().item.name.ToString();
    amount.text = pickableItem.GetItemSlot().amount.ToString();
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    interaction.OnClick(pickableItem);
  }
}
