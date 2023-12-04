using System;
using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
  public static GameManager Instance;

  public UnityEvent<States> OnStateChanged;

  public enum States
  {
    Waiting,
    countDownToStart,
    GamePlaying,
    GameOver
  }

  [SerializeField] private float countDownTime = 3.0f;
  [SerializeField] private float gamePlayTime = 120.0f;
  [SerializeField] private GameObject extractionZonePrefab;
  [SerializeField] private LootSpawner LootSpawnerPrefab;

  [SerializeField] private bool showBoundingBox = true;
  [SerializeField] private Vector3 boundingBoxSize = new Vector3(10, 5, 10);
  [SerializeField] private Vector3 boundingBoxCenter = new Vector3(0, 0, 0);

  private States _state;
  private Dictionary<int, int> playerScores;
  private Dictionary<int, GameObject> players;

  public States State
  {
    get { return _state; }
    set
    {
      _state = value;
      OnStateChanged.Invoke(_state);
    }
  }

  private Timer _timer;

  public Timer CurrentTimer => _timer;

  void Awake()
  {

    // If there is an instance, and it's not me, delete myself.
    if (Instance != null && Instance != this)
    {
      Destroy(this);
    }
    else
    {
      Instance = this;
    }
  }

  void Start()
  {
    OnStateChanged.AddListener((state) =>
    {
      switch (state)
      {
        case States.Waiting:
          _timer = gameObject.AddComponent<Timer>();
          _timer.Init(() =>
           {
             State = States.countDownToStart;
           }, 1f, 0f);
          break;
        case States.countDownToStart:
          _timer = gameObject.AddComponent<Timer>();
          _timer.Init(() =>
          {
            State = States.GamePlaying;
          }, countDownTime, 0f);
          break;
        case States.GamePlaying:
          _timer = gameObject.AddComponent<Timer>();
          _timer.Init(() =>
          {
            State = States.GameOver;
          }, gamePlayTime, 0f);

          Timer extractionTimer = gameObject.AddComponent<Timer>();
          extractionTimer.Init(() =>
          {
              for(int i=0; i<3 ; i++)
              {
            SpawnExtractionZone();
              }
          }, gamePlayTime / 2, 0f);
          break;
        case States.GameOver:
          break;
      }
    });

    SpawnLoots(100);

    State = States.Waiting;
    players = new Dictionary<int, GameObject>();
    playerScores = new Dictionary<int, int>();
  }

  public void AddPlayer(int playerId, GameObject playerObject)
  {
    players.Add(playerId, playerObject);
    playerScores.Add(playerId, 0);
  }

  public void RemovePlayer(int playerId)
  {
    players.Remove(playerId);
    playerScores.Remove(playerId);
    HUD.Instance.UpdateScoreboard(playerScores);
  }

  public void AddScore(int playerId, int score)
  {
    if (!playerScores.ContainsKey(playerId))
    {
      playerScores[playerId] = 0;
    }
    playerScores[playerId] += score;
    HUD.Instance.UpdateScoreboard(playerScores);
  }

    private void SpawnLoots(int amount = 20)
    {
        for(int i=0; i < amount ; i++)
        {
            SpawnAtRandomAndSnapToGround(LootSpawnerPrefab.gameObject, 1f);
        }

    }

    private void SpawnExtractionZone()
    {
            SpawnAtRandomAndSnapToGround(extractionZonePrefab, -0.5f);
    }

    private void SpawnAtRandomAndSnapToGround(GameObject prefab, float snapOffset)
    {
        GameObject spawnedObject = Instantiate(prefab);

        // Generate a random point within the bounding box
        Vector3 randomPoint = new Vector3(
            UnityEngine.Random.Range(boundingBoxCenter.x - boundingBoxSize.x / 2, boundingBoxCenter.x + boundingBoxSize.x / 2),
            10f,
            UnityEngine.Random.Range(boundingBoxCenter.z - boundingBoxSize.z / 2, boundingBoxCenter.z + boundingBoxSize.z / 2)
        );

        // Snap the extraction zone to the ground using a raycast
        if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, float.MaxValue))
        {
            spawnedObject.transform.position = hit.point + Vector3.up * snapOffset;
            //spawnedObject.transform.rotation = Quaternion.FromToRotation(spawnedObject.transform.up, hit.normal) * spawnedObject.transform.rotation;
        }
        else
        {
            Debug.LogWarning("No ground found for extraction zone at random point " + randomPoint);
            Destroy(spawnedObject);
        }
    }

  public void GameOver()
  {
    State = States.GameOver;
  }

  private void OnDrawGizmos()
  {
    if (showBoundingBox)
    {
      Gizmos.color = Color.white;
      Gizmos.DrawWireCube(boundingBoxCenter, boundingBoxSize);
    }
  }

}
