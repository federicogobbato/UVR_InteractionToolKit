using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace UVR.NetworkPhoton
{
    /// <summary>
    /// Game manager.
    /// Connects and watch Photon Status, Instantiate Player
    /// Deals with quiting the room and the game
    /// Deals with level loading (outside the in room synchronization)
    /// </summary>
    public class PUN_GameManager : MonoBehaviourPunCallbacks
    {

        #region Public Variables

        static public PUN_GameManager Instance;

        #endregion

        [SerializeField]
        protected List<PlayerType> m_PlayerTypes;

        protected PlayerType m_PlayerTypeCreated;
        protected GameObject m_PlayerCreated;

        private bool m_AllowMasterClientSwitch = false; // Will be TRUE just for the first client connected

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            Instance = this;

            // in case we started this demo with the wrong scene being active, simply load the menu scene
            if (!PhotonNetwork.IsConnected)
            {
                SceneManager.LoadScene(0);
                return;
            }

            CreatePlayer();
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            // "back" button of phone equals "Escape". quit app if that's pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                QuitApplication();
            }
        }

        #endregion

        #region Photon Messages

        /// <summary>
        /// Called when a Photon Player got connected. We need to then load a bigger scene.
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}  {1}", other.NickName, PhotonNetwork.CurrentRoom.ToString()); // not seen if you're the player connecting
        }


        /// <summary>
        /// Called when a Photon Player got disconnected. We need to load a smaller scene.
        /// </summary>
        /// <param name="other">Other.</param>
        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}  {1}", other.NickName, PhotonNetwork.CurrentRoom.ToString()); // seen when other disconnects

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("IsMasterClient {0}", PhotonNetwork.MasterClient.NickName);
            }
        }


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }


        /// <summary>
        /// Called after disconnecting from the Photon server. It could be a failure or intentional
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            DestroyRoom();
        }


        /// <summary>
        /// Called after switching to a new MasterClient when the current one leaves.
        /// </summary>
        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            Debug.Log("OnMasterClientSwitched()");

            // If the MasterClient leave the room all the other clients must leave the room
            if (!m_AllowMasterClientSwitch)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        #endregion

        #region Public Methods

        public void LeaveRoom()
        {
            DestroyRoom();
            PhotonNetwork.LeaveRoom();
        }


        public void QuitApplication()
        {
            Application.Quit();
        }

        #endregion

        #region Private Methods


        void DestroyRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogFormat("The MasterClient {0} is leaving the room", PhotonNetwork.MasterClient.NickName);

                Room currentRoom = PhotonNetwork.CurrentRoom;
                currentRoom.IsOpen = false;
                currentRoom.IsVisible = false;
                currentRoom.PlayerTtl = 0;
                currentRoom.EmptyRoomTtl = 0;
            }
        }


        protected virtual void CreatePlayer()
        {
            if(PUN_BasePlayerManager.LocalPlayerInstance == null)
            {
                PUN_BasePlayerManager playerPrefab = null;
                PlayerTypeSelector.TypePlayer type = PlayerTypeSelector.TypePlayer.PC;

                if (PlayerTypeSelector.Instance != null)
                {
                    m_PlayerTypeCreated = m_PlayerTypes.Find(x => x.Type == PlayerTypeSelector.Instance.PlayerTypeChosen);
                }
                else
                {
                    m_PlayerTypeCreated = m_PlayerTypes.Find(x => x.Type == PlayerTypeSelector.TypePlayer.PC);
                }

                if (m_PlayerTypeCreated != null)
                {
                    playerPrefab = m_PlayerTypeCreated.PlayerPrefab;
                    type = m_PlayerTypeCreated.Type;

                    if (playerPrefab == null)
                    {
                        Debug.LogError("<Color=Red><b>Missing</b></Color> PlayerPrefab Reference. Please set it up.", this);
                    }
                    else
                    {
                        Debug.Log("We are Instantiating LocalPlayer on " + SceneManagerHelper.ActiveSceneName);
                        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                        m_PlayerCreated = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                    }

                    foreach (var player in m_PlayerTypes)
                    {
                        player.PlayerExtra.SetActive(player.Type == type);
                    }
                }
                else
                {
                    Debug.LogError("PlayerTypeCreated not available");
                }
            }
            else
            {
                Debug.Log("LocalPlayerInstance already exist");
            }
        }
    }

    #endregion

}



