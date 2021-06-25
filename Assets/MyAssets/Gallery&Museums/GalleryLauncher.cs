
using Photon.Pun;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UVR.NetworkPhoton;


public class GalleryLauncher : PUN_Launcher
{

    [Header("Custom")]
    [SerializeField] private GameObject m_ContainerListRooms;
    [SerializeField] private GameObject m_RoomButtonPrefab;
    [SerializeField] private List<string> m_RoomTemplates = new List<string>();

    [SerializeField] private Recorder m_Recorder;

    private Button m_ButtonSelected;

    // Start is called before the first frame update
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

        m_RoomLevelToLoad = "";

        Debug.Log("List of scenes:");

        int sceneCount = SceneManager.sceneCountInBuildSettings;
        List<string> scenes = new List<string>();

        if (!m_RoomButtonPrefab)
        {
            Debug.LogError("RoomButtonPrefab is null");
            return;
        }

        for (int i = 1; i < sceneCount; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string roomFound = m_RoomTemplates.Find(x => path.Contains(x));

            Debug.Log(path + " " + roomFound);

            if (!string.IsNullOrEmpty(roomFound))
            {
                GameObject button = Instantiate(m_RoomButtonPrefab, m_ContainerListRooms.transform);
                button.GetComponentInChildren<Text>().text = roomFound;
                button.GetComponent<Button>().onClick.AddListener(()=> RoomTemplateSelected(roomFound));
            }
        }
    }


    public void RoomTemplateSelected(string nameScene)
    {
        m_RoomLevelToLoad = nameScene;

        if(m_ButtonSelected)
            m_ButtonSelected.gameObject.GetComponent<Image>().color = Color.white;

        m_ButtonSelected = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        m_ButtonSelected.gameObject.GetComponent<Image>().color = Color.red;
    }


    public override void Connect()
    {
        if (string.IsNullOrEmpty(RoomNameToCreate))
        {
            Debug.LogError("You must set a Room name.");
            return;
        }

        base.Connect();
    }
}
