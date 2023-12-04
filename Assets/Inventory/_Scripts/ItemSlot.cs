
[System.Serializable]
public class ItemSlot
{
  public Item item;
  public int amount;

  public ItemSlot(Item item, int amount)
  {
    this.item = item;
    this.amount = amount;
  }
}
