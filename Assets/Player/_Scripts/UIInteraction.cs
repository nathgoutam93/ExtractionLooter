using System.Collections.Generic;
using UnityEngine;

public class UIInteraction : MonoBehaviour
{
  [SerializeField] private Transform itemSlotContainer;
  [SerializeField] private Interactor _interactor;
  [SerializeField] private GameObject itemSlotPrefab;
  [SerializeField] private GameObject ScrollView;

  private void Start()
  {
    _interactor.OnInteractableChange += HandleInteractable;
  }

  private void HandleInteractable(object sender, Interactor.OnInteractableChangeArgs e)
  {
    UpdateUI(e.currentInteractables);
  }

  public void UpdateUI(List<IInteractable> interactables)
  {
    foreach (Transform child in itemSlotContainer)
    {
      Destroy(child.gameObject);
    }

    for (int i = 0; i < interactables.Count; i++)
    {
      var pickable = (PickableItem)interactables[i];
      GameObject newItemSlot = Instantiate(itemSlotPrefab, itemSlotContainer);
      newItemSlot.GetComponent<UIPickableSlot>().Initialize(pickable, this);
    }

    ScrollView.SetActive(interactables.Count > 0);
  }

  public void OnClick(PickableItem item)
  {
    item.Interact(_interactor);
  }

}