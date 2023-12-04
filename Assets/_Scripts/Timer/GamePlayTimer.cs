using System;
using UnityEngine;
using TMPro;

public class GamePlayTimer : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI gameplayTimerLabel;

  void Start()
  {
    GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
  }

  void Update()
  {
    TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.CurrentTimer.CurrentTime);
    gameplayTimerLabel.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
  }

  private void OnGameStateChanged(GameManager.States state)
  {
    if (state == GameManager.States.GamePlaying)
    {
      gameplayTimerLabel.gameObject.SetActive(true);
    }
    else
    {
      gameplayTimerLabel.gameObject.SetActive(false);
    }
  }
}
