using UnityEngine;
using UnityEngine.UI;

public class UIPlayerStats : MonoBehaviour
{

  [SerializeField] private Image healthBar;
  [SerializeField] private Image staminaBar;

  [SerializeField] private PlayerStats playerStats;

  void Update()
  {
    healthBar.fillAmount = playerStats.GetHealthPercent();
    staminaBar.fillAmount = playerStats.GetStaminaPercent();
  }

}
