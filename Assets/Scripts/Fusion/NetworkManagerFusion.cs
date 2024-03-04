using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Fusion.Sockets;
using System;
using UnityEngine.SceneManagement;

public class NetworkManagerFusion : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManagerFusion Instance { get; private set; }
    public NetworkRunner SessionRunner { get; private set; }
    public GameObject runnerPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Debug.Log("Instance will not be destroyed");
        }
    }

    // Start is called before the first frame update
    async void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void StartSharedSession()
    {
        // Create Runner
        CreateRunner();

        // Load Scene
        await LoadScene();

        // Connect to the Session
        await Connect();
    }

    public async Task LoadScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MultiGameSceneFusion");

        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    public void LoadSoloGame()
    {
        SceneManager.LoadScene("SoloGameScene");
    }

    public void LoadMultiplayerGame()
    {
        StartSharedSession();
    }

    public void CreateRunner()
    {
        SessionRunner = Instantiate(runnerPrefab, transform).GetComponent<NetworkRunner>();

        SessionRunner.AddCallbacks(this);
        Debug.Log("Session runner was created");
    }

    private async Task Connect()
    {
        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "TheSession",
            SceneManager = GetComponent<NetworkSceneManagerDefault>()
        };

        var result = await SessionRunner.StartGame(args);

        if (result.Ok)
        {
            Debug.Log("StartGame successfully");
        } else
        {
            Debug.Log(result.ErrorMessage);
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("A new player joined");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Runner shuts down");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
       
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
       
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
       
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
       
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
       
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        
    }
}
