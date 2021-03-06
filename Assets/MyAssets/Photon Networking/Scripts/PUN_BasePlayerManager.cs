using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UVR.NetworkPhoton
{
    public class PUN_BasePlayerManager : MonoBehaviourPunCallbacks
    {
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        public void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instanciation when levels are synchronized
            if (photonView.AmOwner)
            {
                LocalPlayerInstance = gameObject;
            }

            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(gameObject);
        }

        [PunRPC]
        public void RemoteLeaveRoom()
        {
            Debug.Log("I HAVE TO LEAVE THE ROOM");
            PhotonNetwork.LeaveRoom();
        }
    }

}
