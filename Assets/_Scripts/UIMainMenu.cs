using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class UIMainMenu : MonoBehaviour
{

  [Header("References")]
  [SerializeField] private Button PlayBtn;
  [SerializeField] private Button TrainingBtn;
  [SerializeField] private TextMeshProUGUI findMatchButtonText;
  [SerializeField] private TextMeshProUGUI queueTimerText;
  [SerializeField] private TextMeshProUGUI queueStatusText;

  private float m_TimeInQueue;
  private bool m_IsMatchmaking;
  private bool m_IsCancelling;
  private ClientGameManager m_GameManager;


  // Start is called before the first frame update
  void Start()
  {

    if (ClientSingleton.Instance == null) { return; }

    queueStatusText.text = string.Empty;
    queueTimerText.text = string.Empty;

    m_GameManager = ClientSingleton.Instance.Manager;

    PlayBtn.onClick.AddListener(FindMatchPressed);

    TrainingBtn.onClick.AddListener(() =>
    {
      SceneManager.LoadScene("TrainingGround");
    });
  }

  private void Update()
  {
    if (m_IsMatchmaking && !m_IsCancelling)
    {
      m_TimeInQueue += Time.deltaTime;
      TimeSpan ts = TimeSpan.FromSeconds(m_TimeInQueue);
      queueTimerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
    }
    else
    {
      queueTimerText.text = string.Empty;
    }
  }

  public async void FindMatchPressed()
  {
    if (m_IsCancelling) { return; }

    if (m_IsMatchmaking)
    {
      queueStatusText.text = "Cancelling...";
      m_IsCancelling = true;

      await m_GameManager.CancelMatchmaking();

      m_IsCancelling = false;
      m_IsMatchmaking = false;
      findMatchButtonText.text = "Play";
      queueStatusText.text = string.Empty;
      return;
    }

    _ = m_GameManager.MatchmakeAsync(OnMatchMade);

    findMatchButtonText.text = "Cancel";
    queueStatusText.text = "Matchmaking...";
    m_IsMatchmaking = true;
    m_TimeInQueue = 0f;
  }

  private void OnMatchMade(MatchmakerPollingResult result)
  {
    switch (result)
    {
      case MatchmakerPollingResult.Success:
        queueStatusText.text = "Connecting";
        break;
      case MatchmakerPollingResult.TicketCreationError:
        queueStatusText.text = "TicketCreationError";
        break;
      case MatchmakerPollingResult.TicketCancellationError:
        queueStatusText.text = "TicketCancellationError";
        break;
      case MatchmakerPollingResult.TicketRetrievalError:
        queueStatusText.text = "TicketRetrievalError";
        break;
      case MatchmakerPollingResult.MatchAssignmentError:
        queueStatusText.text = "MatchAssignmentError";
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(result), result, null);
    }
  }
}
