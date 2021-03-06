using UnityEngine;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private NET_LobbyNetworkManager networkManager = null;

    [Header("UI")]
    [SerializeField] private GameObject landingPagePanel = null;

    public void HostLobby()
    {
        networkManager.StartHost();

        landingPagePanel.SetActive(false);
    }

    public void SetVR()
    {
        ActivatorVR.Instance.SetVR();
    }
}

