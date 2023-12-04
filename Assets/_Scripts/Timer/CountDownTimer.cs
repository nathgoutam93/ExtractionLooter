using System;
using UnityEngine;
using TMPro;

public class CountDownTimer : MonoBehaviour
{
  [SerializeField] private TextMeshProUGUI countdownTimerLabel;

  void Start()
  {
    GameManager.Instance.OnStateChanged.AddListener(OnGameStateChanged);
  }

  void Update()
  {
    TimeSpan timeSpan = TimeSpan.FromSeconds(GameManager.Instance.CurrentTimer.CurrentTime);
    countdownTimerLabel.text = string.Format("{0:D1}", timeSpan.Seconds);
  }

  private void OnGameStateChanged(GameManager.States state)
  {
    if (state == GameManager.States.countDownToStart)
    {
      countdownTimerLabel.gameObject.SetActive(true);
    }
    else
    {
      countdownTimerLabel.gameObject.SetActive(false);
    }
  }
}
