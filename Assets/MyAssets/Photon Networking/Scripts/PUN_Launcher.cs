using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;

namespace UVR.NetworkPhoton
{

#pragma warning disable 649

    /// <summary>
    /// Launch manager. Connect, join a random room or create one if none or all full.
    /// </summary>
    public class PUN_Launcher : MonoBehaviourPunCallbacks
    {

        #region Public Fields

        static public PUN_Launcher Instance;

        /// <summary>
        /// Name of the room the client is gonna create
        /// </summary>
        static public string RoomNameToCreate = null;

        /// <summary>
        /// Name of the room a client wanna join (different from RoomNameToCreate)
        /// </summary>
        static public string RoomNameToJoin = null;

        #endregion

        #region Protected Fields

        [Tooltip("The client connects to the PUN server when the scene start")]
        [SerializeField]
        bool ConnectToServerAtStart = false;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        protected GameObject m_ControlPanel;

        [Tooltip("The Ui Text to inform the user about the connection progress")]
        [SerializeField]
        protected Text m_FeedbackText;

        [Tooltip("The maximum number of players per room")]
        [SerializeField]
        protected byte m_MaxPlayersPerRoom = 4;

        [Tooltip("The UI Loader Anime")]
        [SerializeField]
        protected LoaderAnime m_LoaderAnime;

        [SerializeField]
        protected bool m_CanCreateRoom = true;

        /// <summary>
        /// Unity name scene to load when create a room
        /// </summary>
        static protected string m_RoomLevelToLoad = "PunBasics-Room";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon, 
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        protected bool m_IsConnecting = false;

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        protected string m_GameVersion = "1";


        #endregion

        #region MonoBehaviour CallBacks

        protected void Awake()
        {
            Instance = this;

            if (m_LoaderAnime == null)
            {
                Debug.LogWarning("<Color=Red><b>Missing</b></Color> loaderAnime Reference.", this);
            }

            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;

            ////Profile = (PUN_ClientProfile)ScriptableObject.CreateInstance(typeof(PUN_ClientProfile));
        }


        protected void Start()
        {         
            if (ConnectToServerAtStart)
            {
                ConnectToServer();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Connect to the Photon Online Server
        /// </summary>
        protected void ConnectToServer()
        {
            Debug.Log("ConnectToServer");
            LogFeedback("Connecting...");

            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = this.m_GameVersion;
        }

        /// <summary>
        /// Try to join a random room or one that already exist
        /// </summary>
        protected void TryJoinRoom()
        {
            Debug.Log("CustomJoinRoom");
            LogFeedback("Joining Room...");

            // #Critical we need at this point to attempt joining a Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            //if(string.IsNullOrEmpty(RoomNameToJoin))
            //{
            //    Debug.Log("Join random room");
            //    PhotonNetwork.JoinRandomRoom();
            //}
            //else
            //{
            //    Debug.Log("Join room: " + RoomNameToJoin);
            //    PhotonNetwork.JoinRoom(RoomNameToJoin);
            //}

            Debug.Log("Join random room");
            PhotonNetwork.JoinRandomRoom();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the connection process. 
        /// - If already connected, we attempt joining a room
        /// - if not yet connected, connect this application instance to the server
        /// </summary>
        public virtual void Connect()
        {
            if (string.IsNullOrEmpty(m_RoomLevelToLoad) ||
                string.IsNullOrEmpty(PhotonNetwork.NickName))
            {
                Debug.LogError("NickName or RoomTemplate are null");
                return;
            }

            // we want to make sure the log is clear everytime we connect, we might have several failed attempted if connection failed.
            m_FeedbackText.text = "";

            // keep track of the will to join a room, because when we come back from the game we will get a callback that we are connected, so we need to know what to do then
            m_IsConnecting = true;

            // hide the Play button for visual consistency
            m_ControlPanel.SetActive(false);

            // start the loader animation for visual effect.
            if (m_LoaderAnime != null)
            {
                m_LoaderAnime.StartLoaderAnimation();
            }

            // we check if we are connected or not, we join if we ar , else we initiate the connection to the server.
            if (PhotonNetwork.IsConnected)
            {
                TryJoinRoom();
            }
            else
            {
                ConnectToServer();
            }
        }


        /// <summary>
        /// Logs the feedback in the UI view for the player, as opposed to inside the Unity Editor for the developer.
        /// </summary>
        /// <param name="message">Message.</param>
        protected void LogFeedback(string message)
        {
            // we do not assume there is a feedbackText defined.
            if (m_FeedbackText == null)
            {
                return;
            }

            // add new messages as a new line and at the bottom of the log.
            m_FeedbackText.text += System.Environment.NewLine + message;
        }

        #endregion

        #region MonoBehaviourPunCallbacks CallBacks


        /// <summary>
        /// Called after the connection to the master is established and authenticated
        /// </summary>
        public override void OnConnectedToMaster()
        {
            Debug.Log("OnConnectedToMaster");

            // we don't want to do anything if we are not attempting to join a room. 
            // this case where isConnecting is false is typically when you lost or quit the game, when this level is loaded, OnConnectedToMaster will be called, in that case
            // we don't want to do anything.
            if (m_IsConnecting)
            {
                LogFeedback("OnConnectedToMaster: Next -> try to Join Random Room");
                Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room.\n Calling: PhotonNetwork.JoinRandomRoom() or PhotonNetwork.JoinRoom(...name room); Operation will fail if no room found");

                // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
                TryJoinRoom();
            }
        }


        /// <summary>
        /// Called when a JoinRandomRoom() call failed. The parameter provides ErrorCode and message.
        /// Most likely all rooms are full or no rooms are available. 
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            LogFeedback("<Color=Red>OnJoinRandomFailed</Color>: Next -> Create a new Room");
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            if (m_CanCreateRoom)
            {
                string fullRoomName = string.Format("{0} - {1} ({2})", RoomNameToCreate, PhotonNetwork.NickName, m_RoomLevelToLoad);
                Debug.Log("RoomName: " + fullRoomName);

                // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
                PhotonNetwork.CreateRoom(fullRoomName, new RoomOptions { MaxPlayers = this.m_MaxPlayersPerRoom });
            }
        }


        /// <summary>
        /// Called when a previous JoinRoom call failed on the server.
        /// The most common causes are that a room is full or does not exist (due to someone else being faster or closing the room).
        /// </summary>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            LogFeedback("<Color=Red>OnJoinRoomFailed</Color>: ");
            Debug.LogError("PUN Basics Tutorial/Launcher:OnJoinRoomFailed() was called by PUN.");
            Debug.LogError(returnCode + ": " + message);
        }


        /// <summary>
        /// Called after disconnecting from the Photon server.
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            LogFeedback("<Color=Red>OnDisconnected</Color> " + cause);
            Debug.LogError("PUN Basics Tutorial/Launcher:Disconnected");

            // #Critical: we failed to connect or got disconnected. There is not much we can do. Typically, a UI system should be in place to let the user attemp to connect again.
            m_LoaderAnime.StopLoaderAnimation();

            m_IsConnecting = false;
            m_ControlPanel.SetActive(true);

        }


        /// <summary>
        /// Called when entering a room (by creating or joining it). Called on all clients (including the Master Client).
        /// </summary>
        public override void OnJoinedRoom()
        {
            LogFeedback("<Color=Green>OnJoinedRoom</Color> with " + PhotonNetwork.CurrentRoom.PlayerCount + " Player(s)");
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.\nFrom here on, your game would be running.");

            // #Critical: We only load if we are the first player, else we rely on PhotonNetwork.AutomaticallySyncScene to sync our instance scene.
            if (m_CanCreateRoom && PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the Room");

                // #Critical
                // Load the Room Level.        
                PhotonNetwork.LoadLevel(m_RoomLevelToLoad);
            }
        }

        #endregion
    }

}
