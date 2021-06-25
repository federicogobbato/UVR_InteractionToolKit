using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NET_SpawnSystem : NetworkBehaviour
{
    public GameObject VRPref;
    public GameObject PlayerPref;

    private NET_LobbyNetworkManager m_NetManager;
    private NET_LobbyNetworkManager NetManager
    {
        get
        {
            if (m_NetManager != null) { return m_NetManager; }
            return m_NetManager = NetworkManager.singleton as NET_LobbyNetworkManager;
        }
    }

    public override void OnStartServer()
    {
        NET_LobbyNetworkManager.OnServerReadied += SpawnPlayer;
    }

    [ServerCallback]
    private void OnDestroy()
    {
        NET_LobbyNetworkManager.OnServerReadied -= SpawnPlayer;
    }

    [Server]
    private void SpawnPlayer(NetworkConnection conn)
    {
        GameObject prefab = null;
        var gamePlayer = conn.identity.GetComponent<NET_GamePlayer>();

        if (gamePlayer)
        {
            if (gamePlayer.IsVR)
            {
                prefab = VRPref;
            }
            else
            {
                prefab = PlayerPref;
            }
        }

        GameObject player = Instantiate(prefab);
        NetworkServer.Spawn(player, conn);
    }
}
