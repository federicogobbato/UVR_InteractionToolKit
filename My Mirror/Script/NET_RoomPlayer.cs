using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NET_RoomPlayer : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[4];
    [SerializeField] private TMP_Text[] playerVRTexts = new TMP_Text[4];
    [SerializeField] private Button startGameButton = null;

    private bool m_IsLeader;
    public bool IsLeader
    {
        set
        {
            m_IsLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
    }

    private NET_LobbyNetworkManager m_NetManager;
    protected NET_LobbyNetworkManager NetManager
    {
        get
        {
            if (m_NetManager != null) { return m_NetManager; }
            return m_NetManager = NetworkManager.singleton as NET_LobbyNetworkManager;
        }
    }

    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar(hook = nameof(HandleDisplayNameChanged))] 
    public string DisplayName = "Loading...";
    [SyncVar]
    public bool IsVR = false;


    //Automatically called when SyncVar are changed
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateListOfPlayersDisplayed();
    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateListOfPlayersDisplayed();


    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNameInput.DisplayName);
        CmdSetVR(ActivatorVR.Instance.isVR);

        lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        NetManager.RoomPlayers.Add(this);

        UpdateListOfPlayersDisplayed();
    }


    ////public override void OnNetworkDestroy()
    ////{
    ////    NetManager.RoomPlayers.Remove(this);

    ////    UpdateListOfPlayersDisplayed();
    ////}

    public override void OnStopClient()
    {
        NetManager.RoomPlayers.Remove(this);

        UpdateListOfPlayersDisplayed();

        base.OnStopClient();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
    }


    private void UpdateListOfPlayersDisplayed()
    {
        if (!hasAuthority)
        {
            foreach (var player in NetManager.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateListOfPlayersDisplayed();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < NetManager.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = NetManager.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = NetManager.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" :
                "<color=red>Not Ready</color>";
        }

        for (int i = 0; i < NetManager.RoomPlayers.Count; i++)
        {
            playerVRTexts[i].text = NetManager.RoomPlayers[i].IsVR ?
                "<color=green>VR</color>" :
                "<color=red>VR</color>";
        }
    }


    public void HandleReadyToStart(bool readyToStart)
    {
        if (!m_IsLeader) { return; }

        startGameButton.interactable = readyToStart;
    }


    [Command]
    private void CmdSetVR(bool isVR)
    {
        IsVR = isVR;
    }


    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        NetManager.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (NetManager.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        NetManager.StartGame();
    }
}
