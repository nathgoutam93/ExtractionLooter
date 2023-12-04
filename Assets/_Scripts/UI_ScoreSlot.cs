using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreSlotUI : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI playerName;
  [SerializeField] private TextMeshProUGUI score;

  public void SetPlayerName(string name)
  {
    playerName.text = name;
  }

  public void SetScore(string score)
  {
    playerName.text = score;
  }
}
