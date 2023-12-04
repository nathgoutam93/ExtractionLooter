using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Inventory : MonoBehaviour
{
  [SerializeField] private int inventorySize = 100;
  public UnityEvent OnItemsChanged;
  public UnityEvent<int> OnValueChanged;
  public List<ItemSlot> itemSlots = new List<ItemSlot>();
  private int _currentOcupiedSpace = 0;
  private int _currentInventoryValue;

  public int InventoryValue
  {
    get { return _currentInventoryValue; }
    set
    {
      _currentInventoryValue = value;
      OnValueChanged.Invoke(_currentInventoryValue);
    }
  }

  private void Start()
  {
    OnItemsChanged.AddListener(() =>
    {

      int totalValue = 0;

      itemSlots.ForEach((itemSlot) =>
      {
        totalValue += itemSlot.item.value * itemSlot.amount;
      });

      InventoryValue = totalValue;
    });
  }

  private int AvaiableSpace()
  {
    return inventorySize - _currentOcupiedSpace;
  }

  private bool IsFull()
  {
    return _currentOcupiedSpace >= inventorySize;
  }

  public bool AddItem(Item item, int amount = 1)
  {

    if (IsFull() || (item.size * amount) > AvaiableSpace())
    {
      return false;
    }

    if (!item.canStack)
    {
      for (int i = 0; i < amount; i++)
      {
        itemSlots.Add(new ItemSlot(item, 1));
        _currentOcupiedSpace += item.size;
      }

      OnItemsChanged.Invoke();
      return true;
    }

    ItemSlot itemSlot = itemSlots.Find(slot => slot.item == item && slot.amount < item.maxStackSize);

    if (itemSlot == null)
    {
      int amountLeft = amount;

      while (amountLeft > 0)
      {
        int amountAdded = Mathf.Min(item.maxStackSize, amountLeft);
        itemSlots.Add(new ItemSlot(item, amountAdded));
        _currentOcupiedSpace += item.size * amountAdded;

        amountLeft = amountLeft - amountAdded;
      }

      OnItemsChanged.Invoke();
      return true;
    }
    else
    {
      int spaceLeft = item.maxStackSize - itemSlot.amount;

      if (amount <= spaceLeft)
      {
        itemSlot.amount += amount;
        _currentOcupiedSpace += item.size * amount;

        OnItemsChanged.Invoke();
        return true;
      }
      else
      {
        int leftAmount = amount - spaceLeft;

        itemSlot.amount = item.maxStackSize;
        _currentOcupiedSpace += item.size * spaceLeft;

        int amountLeft = leftAmount;

        while (amountLeft > 0)
        {
          int amountAdded = Mathf.Min(item.maxStackSize, amountLeft);
          itemSlots.Add(new ItemSlot(item, amountAdded));
          _currentOcupiedSpace += item.size * amountAdded;

          amountLeft = amountLeft - amountAdded;
        }

        OnItemsChanged.Invoke();
        return true;
      }
    }
  }

  public bool RemoveItem(ItemSlot itemSlot, int amount = 1)
  {
    if (itemSlot != null)
    {
      if (itemSlot.amount >= amount)
      {
        itemSlot.amount -= amount;
        _currentOcupiedSpace -= itemSlot.item.size * amount;

        if (itemSlot.amount == 0)
        {
          itemSlots.Remove(itemSlot);
        }

        OnItemsChanged.Invoke();
        return true;
      }
    }

    return false;
  }

  public void ConsumeItem(ItemSlot itemSlot)
  {
    if (itemSlot.item.isConsumable)
    {
      PlayerStats playerStats = GetComponent<PlayerStats>();

      if (playerStats == null) return;

      playerStats.AddHealth(itemSlot.item.health);

      RemoveItem(itemSlot);
    }
  }
}
