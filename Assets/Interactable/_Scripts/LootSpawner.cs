using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootSpawner : MonoBehaviour
{
  [SerializeField] private List<PickableItem> _itemList = new List<PickableItem>();

  private PickableItem GetDroppedItem()
  {
    int randomNumber = Random.Range(1, 101);

    List<PickableItem> possibleItems = new List<PickableItem>();

    foreach (PickableItem item in _itemList)
    {
      if (randomNumber <= item.InventoryItem.dropRate)
      {
        possibleItems.Add(item);
      }
    }

    if (possibleItems.Count > 0)
    {
      PickableItem dropItem = possibleItems[Random.Range(0, possibleItems.Count)];

      return dropItem;
    }

    return null;
  }

  public void InstantiateItem()
  {
    PickableItem droppedItem = GetDroppedItem();
    if (droppedItem != null)
    {
      Instantiate(droppedItem, transform.position, Quaternion.identity);
    }
  }

  private void Start()
  {
    InstantiateItem();
    Destroy(gameObject);
  }

}
