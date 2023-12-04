using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class HUD : MonoBehaviour
{
  public static HUD Instance { get; private set; }

  [SerializeField] private TextMeshProUGUI gameOverLabel;

  [SerializeField] public GameObject scoreSlotPrefab;
  [SerializeField] public Transform scoreboardContainer;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
  }

  void Start()
  {
    GameManager.Instance.OnStateChanged.AddListener((state) =>
    {
      if (state == GameManager.States.GameOver)
      {
        gameOverLabel.gameObject.SetActive(true);
      }
    });
  }

  void Update()
  {
  }

  public void UpdateScoreboard(Dictionary<int, int> playerScores)
  {
    // Destroy any existing score slots
    foreach (Transform child in scoreboardContainer)
    {
      Destroy(child.gameObject);
    }

    // Create a new score slot for each player
    foreach (int playerId in playerScores.Keys)
    {
      GameObject scoreSlot = Instantiate(scoreSlotPrefab, scoreboardContainer);
      ScoreSlotUI scoreUISlot = scoreSlot.GetComponent<ScoreSlotUI>();
      scoreUISlot.SetPlayerName("Player " + playerId);
      scoreUISlot.SetScore(playerScores[playerId].ToString());
    }
  }
}
