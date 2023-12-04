using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Proyecto26;
using System.Collections.Generic;

public class ClientGameManager : IDisposable
{
    public event Action<Matchplayer> MatchPlayerSpawned;
    public event Action<Matchplayer> MatchPlayerDespawned;
    public MatchplayUser User { get; private set; }
    public MatchplayNetworkClient NetworkClient { get; private set; }

    public MatchplayMatchmaker Matchmaker { get; private set; }
    public bool Initialized { get; private set; } = false;

    public ClientGameManager()
    {
        User = new MatchplayUser();
    }

    public async Task InitAsync()
    {
        await UnityServices.InitializeAsync();

        AuthState authenticationResult = await AuthenticationWrapper.DoAuth();

        NetworkClient = new MatchplayNetworkClient();
        Matchmaker = new MatchplayMatchmaker();

        if (authenticationResult == AuthState.Authenticated)
        {
            User.AuthId = AuthenticationWrapper.PlayerID();
        }

        else
        {
            Debug.Log("Unity Authentication Failed.");
            User.AuthId = Guid.NewGuid().ToString();
        }

        Debug.Log($"did Auth?{authenticationResult} {User.AuthId}");

        // // Check if the player prefs has the phone already
        // string phone = PlayerPrefs.GetString("phone");
        // string countryCode = PlayerPrefs.GetString("country_code");

        // if(String.IsNullOrEmpty(phone))
        // {
        //     // promt auth scene
        //     SceneManager.LoadScene("Auth", LoadSceneMode.Single);
        // }
        // else
        // {
        //     // Call thirdparty API to remap the unity authID
        //     Dictionary<string, string> parameters = new Dictionary<string, string>
        //     {
        //         { "country_code", countryCode },
        //         { "phone", phone },
        //         { "authid", User.AuthId }
        //     };

        //     RestClient.Post($"{ThirdPartyAPIs.SERVER_BASE_URL}/re-map", parameters).Then(response =>
        //     {
        //         Debug.Log("Remapped Successfully");
        //     }).Catch(error =>
        //     {
        //         Debug.LogError(error.Message);
        //     });
        // }

        Initialized = true;
    }

    public void BeginConnection(string ip, int port)
    {
        Debug.Log($"Starting networkClient @ {ip}:{port}\nWith : {User}");
        NetworkClient.StartClient(ip, port);
    }

    public void Disconnect()
    {
        NetworkClient.DisconnectClient();
    }

    public async Task MatchmakeAsync(Action<MatchmakerPollingResult> onMatchmakeResponse)
    {
        if (Matchmaker.IsMatchmaking)
        {
            Debug.LogWarning("Already matchmaking, please wait or cancel.");
            return;
        }

        MatchmakerPollingResult matchResult = await GetMatchAsync();
        onMatchmakeResponse?.Invoke(matchResult);
    }

    private async Task<MatchmakerPollingResult> GetMatchAsync()
    {
        Debug.Log($"Beginning Matchmaking with {User}");
        MatchmakingResult matchmakingResult = await Matchmaker.Matchmake(User.Data);

        if (matchmakingResult.result == MatchmakerPollingResult.Success)
        {
            BeginConnection(matchmakingResult.ip, matchmakingResult.port);
        }
        else
        {
            Debug.LogWarning($"{matchmakingResult.result} : {matchmakingResult.resultMessage}");
        }

        return matchmakingResult.result;
    }

    public async Task CancelMatchmaking()
    {
        await Matchmaker.CancelMatchmaking();
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    public void AddMatchPlayer(Matchplayer player)
    {
        MatchPlayerSpawned?.Invoke(player);
    }

    public void RemoveMatchPlayer(Matchplayer player)
    {
        MatchPlayerDespawned?.Invoke(player);
    }

    public void SetGameQueue(GameQueue queue)
    {
        User.QueuePreference = queue;
    }

    public void ExitGame()
    {
        Dispose();
        Application.Quit();
    }

    public void Dispose()
    {
        NetworkClient?.Dispose();
        Matchmaker?.Dispose();
    }
}