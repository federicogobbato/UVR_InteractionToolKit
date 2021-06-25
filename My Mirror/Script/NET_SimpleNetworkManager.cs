using Mirror;
using Mirror.Cloud.Example;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NET_SimpleNetworkManager : NetworkManagerListServer
{
    [SerializeField] private GameObject m_NoVRPlayer;
    [SerializeField] private GameObject m_VrPlayer;


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


    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        GameObject player;
        
        if (ActivatorVR.Instance.isVR)
        {
            player = Instantiate(m_VrPlayer);            
        }
        else
        {
            player = Instantiate(m_NoVRPlayer);
        }

        NetworkServer.AddPlayerForConnection(conn, player);
    }

}
