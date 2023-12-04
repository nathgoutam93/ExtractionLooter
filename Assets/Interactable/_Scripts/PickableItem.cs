using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
  [SerializeField] protected Item _item;
  [SerializeField] protected int _amount = 1;
  [SerializeField] protected string _prompt = "Pick";

  public string InteractionPrompt => _prompt;
  public Item InventoryItem => _item;

  public PickableItem(Item item, int amount)
  {
    this._item = item;
    this._amount = amount;
  }

  public ItemSlot GetItemSlot()
  {
    var itemSlot = new ItemSlot(_item, _amount);
    return itemSlot;
  }

  public void SetAmount(int amount)
  {
    _amount = amount;
  }

  public bool Interact(Interactor interactor)
  {
    Inventory inventory = interactor.GetComponent<Inventory>();
    Pick(inventory);
    interactor.RemoveInteractable(this);
    return true;
  }

  private void Pick(Inventory inventory)
  {
    if (inventory != null)
    {
      if (inventory.AddItem(_item, _amount))
      {
        // Destroy the item after adding it to the inventory
        Destroy(gameObject);
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    var interactor = other.GetComponent<Interactor>();

    if (interactor != null)
    {
      interactor.AddInteractable(this);
    }
  }

  private void OnTriggerExit(Collider other)
  {
    var interactor = other.GetComponent<Interactor>();

    if (interactor != null)
    {
      interactor.RemoveInteractable(this);
    }
  }
}
