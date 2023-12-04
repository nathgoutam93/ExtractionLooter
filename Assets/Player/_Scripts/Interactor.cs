using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactor : MonoBehaviour
{
  public EventHandler<OnInteractableChangeArgs> OnInteractableChange;
  public class OnInteractableChangeArgs : EventArgs
  {
    public List<IInteractable> currentInteractables;
  }

  private List<IInteractable> _currentInteractables = new List<IInteractable>(0);
  private PlayerInputs input;

  private void Start()
  {
    input = GetComponent<PlayerInputs>();

    input.OnInteractEvent.AddListener(Interact);
  }

  private void Update()
  {
  }

  // public IInteractable CurrentInteractable => _currentInteractable;

  // public void SetCurrentInteractable(IInteractable interactable)
  // {
  //   _currentInteractable = interactable;
  //   OnInteractableChange.Invoke(this, new OnInteractableChangeArgs { currentInteractable = _currentInteractable });
  // }

  // public void ClearCurrentInteractable()
  // {
  //   _currentInteractable = null;
  //   OnInteractableChange.Invoke(this, new OnInteractableChangeArgs { currentInteractable = _currentInteractable });
  // }

  private void Interact()
  {
    if (_currentInteractables.Count > 0)
    {
      _currentInteractables[0].Interact(this);
    }
  }

  public bool AddInteractable(IInteractable interactable)
  {
    _currentInteractables.Add(interactable);
    OnInteractableChange?.Invoke(this, new OnInteractableChangeArgs
    {
      currentInteractables = _currentInteractables
    });

    return true;
  }

  public bool RemoveInteractable(IInteractable interactable)
  {
    bool removed = _currentInteractables.Remove(interactable);

    if (removed)
    {
      OnInteractableChange?.Invoke(this, new OnInteractableChangeArgs
      {
        currentInteractables = _currentInteractables
      });
    }

    return removed;
  }
}
