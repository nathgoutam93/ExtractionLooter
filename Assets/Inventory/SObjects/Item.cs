using UnityEngine;

[CreateAssetMenu(menuName = "Inventory/Inventory Item")]
public class Item : ScriptableObject
{
  public string itemName;
  public Sprite icon;
  public int size;
  public bool canStack;
  public int maxStackSize;
  public PickableItem Prefeb;
  public int dropRate;
  public int value;
  public bool isConsumable;
  public int health;
}