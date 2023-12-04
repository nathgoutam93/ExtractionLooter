using System;
using UnityEngine;

public class Timer : MonoBehaviour
{
  private Action onTimerEnd;
  private float startingTime;
  private bool countDown;
  private bool hasLimit;
  private float timeLimit;

  private float _currentTime;

  public float CurrentTime => _currentTime;

  public void Init(float start, float end)
  {
    this.hasLimit = true;

    this.startingTime = start;
    this.timeLimit = end;

    this.countDown = startingTime > end;
    this._currentTime = startingTime;
  }

  public void Init(Action onTimerEnd, float start, float end, bool isCountDown = true, bool hasLimit = true)
  {
    this.onTimerEnd = onTimerEnd;
    this.startingTime = start;
    this.countDown = isCountDown;
    this.hasLimit = hasLimit;
    this.timeLimit = end;
    this._currentTime = startingTime;
  }

  // Update is called once per frame
  private void Update()
  {

    if (countDown)
    {

      if (hasLimit)
      {
        _currentTime = Mathf.Clamp(_currentTime -= Time.deltaTime, timeLimit, startingTime);
      }
      else
      {
        _currentTime -= Time.deltaTime;
      }
    }

    else
    {
      if (hasLimit)
      {
        _currentTime = Mathf.Clamp(_currentTime += Time.deltaTime, startingTime, timeLimit);
      }
      else
      {
        _currentTime += Time.deltaTime;
      }
    }

    if (hasLimit && (countDown ? _currentTime <= timeLimit : _currentTime >= timeLimit))
    {
      onTimerEnd();
      Destroy(this);
    }

  }
}
