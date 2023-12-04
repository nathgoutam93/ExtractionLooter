using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PlayerStats : MonoBehaviour
{
  [SerializeField] private float maxHealth = 100f;
  [SerializeField] private float maxStamina = 100f;

  [SerializeField] private float healthDecreaseRate = 1f;
  [SerializeField] private float staminaRecoveryRate = 2f;

  private PlayerController player;

  private float currentHealth;
  private float currentStamina;

  void Start()
  {
    currentHealth = maxHealth;
    currentStamina = maxStamina;

    player = GetComponent<PlayerController>();
  }

  void Update()
  {
    DecreaseHealth();
    ManageStamina();

    if (currentHealth <= 0)
    {
      //GameManager.Instance.GameOver();
    }
  }

  public float GetHealthPercent()
  {
    return currentHealth / maxHealth;
  }

  public float GetStaminaPercent()
  {
    return currentStamina / maxStamina;
  }

  private void DecreaseHealth()
  {
    currentHealth -= healthDecreaseRate * Time.deltaTime;
  }

  public void AddHealth(int amount)
  {
    currentHealth += amount;
    if (currentHealth > maxHealth)
    {
      currentHealth = maxHealth;
    }
  }

  private void ManageStamina()
  {
    if (player.IsSprinting)
    {
      currentStamina -= staminaRecoveryRate * Time.deltaTime;
      if (currentStamina <= 0)
      {
        currentStamina = 0;
        player.SetSprinting(false);
      }
    }
    else
    {
      currentStamina += staminaRecoveryRate * Time.deltaTime;
      if (currentStamina > maxStamina)
      {
        currentStamina = maxStamina;
      }
    }
  }
}
