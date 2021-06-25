using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UVR.NetworkPhoton;

public class GalleryClientLauncher : PUN_Launcher
{
    [Header("Custom")]
    [SerializeField] private GameObject m_ContainerListRooms;
    [SerializeField] private GameObject m_RoomButtonPrefab;
    [SerializeField] private Recorder m_Recorder;

    private Dictionary<GameObject, RoomInfo> m_RoomButtonList = new Dictionary<GameObject, RoomInfo>();

    private RoomInfo m_InfoRoomLevelToLoad;

    private Button m_ButtonSelected;

    protected new void Start()
    {
        base.Start();

        if (!m_Recorder)
        {
            m_Recorder = GameObject.FindObjectOfType<Recorder>();
        }

#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }

        for (int i = 0; i < Microphone.devices.Length; ++i)
        {
            LogFeedback(Microphone.devices[i]);
        }

        m_Recorder.UnityMicrophoneDevice = Microphone.devices[0];

        LogFeedback("Microphone : " + m_Recorder.UnityMicrophoneDevice + "  id: " + m_Recorder.PhotonMicrophoneDeviceId);
#endif
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomButtonList();
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomButtonList();

        foreach (var roomInfo in roomList)
        {
            GameObject button = Instantiate(m_RoomButtonPrefab, m_ContainerListRooms.transform);
            m_RoomButtonList.Add(button, roomInfo);

            button.GetComponentInChildren<Text>().text = roomInfo.Name;
            button.GetComponent<Button>().onClick.AddListener(() => RoomTemplateSelected(roomInfo));
        }
    }

    private void ClearRoomButtonList()
    {
        foreach (var roomButton in m_RoomButtonList)
        {
            Destroy(roomButton.Key);
        }

        m_RoomButtonList.Clear();
    }


    public void RoomTemplateSelected(RoomInfo roomInfo)
    {
        m_InfoRoomLevelToLoad = roomInfo;
        RoomNameToJoin = roomInfo.Name;

        if (m_ButtonSelected)
            m_ButtonSelected.gameObject.GetComponent<Image>().color = Color.white;

        m_ButtonSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        m_ButtonSelected.gameObject.GetComponent<Image>().color = Color.red;
    }


    public override void Connect()
    {
        if (string.IsNullOrEmpty(RoomNameToJoin))
        {
            Debug.LogError("You must choose the Room.");
            return;
        }

        base.Connect();
    }
}
