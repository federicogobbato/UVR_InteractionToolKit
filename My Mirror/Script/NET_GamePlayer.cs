using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NET_GamePlayer : NetworkBehaviour
{
    [SyncVar]
    private string displayName = "Loading...";
    [SyncVar]
    public bool IsVR = false;

    private NET_LobbyNetworkManager m_NetManager;
    protected NET_LobbyNetworkManager NetManager
    {
        get
        {
            if (m_NetManager != null) { return m_NetManager; }
            return m_NetManager = NetworkManager.singleton as NET_LobbyNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);

        NetManager.GamePlayers.Add(this);
    }


    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// </summary>
    public override void OnStopClient()
    {
        NetManager.GamePlayers.Remove(this);

        base.OnStopClient();
    }


    [Server]
    public void SetDisplayName(string displayName)
    {
        this.displayName = displayName;
    }

    [Server]
    public void SetVR(bool isVR)
    {
        IsVR = isVR;
    }

    [Command]
    public void SetOwnerToObject(NetworkIdentity objectIdentity, NetworkIdentity playerIdentity)
    {
        RemoveOwnerToObejct(objectIdentity);
        objectIdentity.AssignClientAuthority(playerIdentity.connectionToClient);
    }

    [Command]
    public void RemoveOwnerToObejct(NetworkIdentity objectIdentity)
    {
        if (objectIdentity.hasAuthority)
            objectIdentity.RemoveClientAuthority();
    }
}
