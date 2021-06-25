using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

/// <summary>
/// Manage how the player are instantiated, what happen when they connect or disconnect...
/// </summary>
public class NET_LobbyNetworkManager : NetworkManager
{
    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;
    public static event Action OnServerStopped;

    [Header("Custom")]
    [SerializeField] private int m_MinPlayers = 1;
    [Scene] [SerializeField] private string m_MenuScene = string.Empty;

    [Header("Spawnables")]
    [SerializeField] private NET_RoomPlayer m_RoomPlayerPrefab = null;
    [SerializeField] private NET_GamePlayer m_GamePlayerPrefab = null;
    [SerializeField] private NET_SpawnSystem m_SpawnSystem = null;

    public List<NET_RoomPlayer> RoomPlayers { get; } = new List<NET_RoomPlayer>();
    public List<NET_GamePlayer> GamePlayers { get; } = new List<NET_GamePlayer>();

    private int m_Level = -1;
    private string m_CurrentScene;


    #region OVERRIDED METHODS 

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("Spawnables").ToList();
    }


    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("Spawnables");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }


    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        OnClientConnected?.Invoke();
    }


    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);
        OnClientDisconnected?.Invoke();
    }


    /// <summary>
    /// Called on the server when a new client connects.
    /// </summary>
    public override void OnServerConnect(NetworkConnection conn)
    {
        //Comment: the current scene on connect to the server have not be the MenuScene
        if (numPlayers >= maxConnections /*|| !m_MenuScene.Contains(SceneManager.GetActiveScene().name)*/)
        {
            conn.Disconnect();
            return;
        }
    }

    /// <summary>
    /// Called on server when the cliet is ready
    /// </summary>
    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }


    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if (m_MenuScene.Contains(SceneManager.GetActiveScene().name))
        {
            bool isLeader = RoomPlayers.Count == 0;

            NET_RoomPlayer roomPlayerInstance = Instantiate(m_RoomPlayerPrefab);

            roomPlayerInstance.GetComponent<NET_RoomPlayer>().IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }


    /// <summary>
    /// Called on the server when a client disconnects.
    /// </summary>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NET_RoomPlayer>();
            
            RoomPlayers.Remove(player);
            NotifyPlayersOfReadyState();
        }

        base.OnServerDisconnect(conn);
    }


    public override void OnStopServer()
    {
        OnServerStopped?.Invoke();

        RoomPlayers.Clear();
        GamePlayers.Clear();
    }


    public override void ServerChangeScene(string newSceneName)
    {
        // From menu to game
        if (m_MenuScene.Contains(SceneManager.GetActiveScene().name) && newSceneName.StartsWith("Level"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;

                ////NET_GamePlayer gameplayerInstance = Instantiate(RoomPlayers[i].IsVR ? m_GamePlayerPrefabVR : m_GamePlayerPrefab);

                NET_GamePlayer gameplayerInstance = Instantiate(m_GamePlayerPrefab);
                gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);
                gameplayerInstance.SetVR(RoomPlayers[i].IsVR);

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject, true);
            }

            m_Level++;
            base.ServerChangeScene(newSceneName + m_Level);
        }
    }


    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith("Level"))
        {
            m_CurrentScene = sceneName;
            NET_SpawnSystem spawnSystem = Instantiate(m_SpawnSystem);
            NetworkServer.Spawn(spawnSystem.gameObject); //the SERVER will be the owner
        }
    }

    #endregion

    private bool IsReadyToStart()
    {
        if (numPlayers < m_MinPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }


    public void StartGame()
    {
        if (m_MenuScene.Contains(SceneManager.GetActiveScene().name))
        {
            if (!IsReadyToStart()) { return; }

            Debug.Log("Game ready to start");
            ServerChangeScene("Level");
        }
    }
}
