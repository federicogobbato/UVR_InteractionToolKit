
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UVR.NetworkPhoton
{

    public class PUN_ProfileInputFields : MonoBehaviour
    {
        [SerializeField]
        InputField m_PlayerNameInputField;

#region Private Constants

        // Store the PlayerPref Key to avoid typos
        const string playerNamePrefKey = "PlayerName";

#endregion

#region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            if (m_PlayerNameInputField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    string defaultName = PlayerPrefs.GetString(playerNamePrefKey);

                    m_PlayerNameInputField.text = defaultName;
                    PhotonNetwork.NickName = defaultName;
                }
            }
        }

#endregion

#region Public Methods

        /// <summary>
        /// Sets the name of the player, and save it in the PlayerPrefs for future sessions.
        /// </summary>
        /// <param name="value">The name of the Player</param>
        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
            }

            PhotonNetwork.NickName = value;
            ////PUN_Launcher.Instance.Profile.NickName = value;

            PlayerPrefs.SetString(playerNamePrefKey, value);
        }


        public void SetPassword(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Password is null or empty");
            }

            ////PUN_Launcher.Instance.Profile.Password = value;
        }


        public void SetRoomName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Room name is null or empty");
            }

            PUN_Launcher.RoomNameToCreate = value;
        }

#endregion
    }
}
