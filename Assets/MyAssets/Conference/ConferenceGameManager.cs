using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UVR.NetworkPhoton;

public class ConferenceGameManager : PUN_GameManager
{
    [SerializeField]
    Transform m_Table;
    [SerializeField]
    List<GameObject> m_Chairs = new List<GameObject>();
 
    Transform m_GoogleCardboardCamera;

    /// <summary>
    /// Set positition of of the partecipant inside the 
    /// </summary>
    protected override void CreatePlayer()
    {
        base.CreatePlayer();

        if (m_PlayerTypeCreated.Type == PlayerTypeSelector.TypePlayer.VR_CARDBOARD)
        {
            Vector3 newPosition = m_Chairs[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.position;
            newPosition.y += 1;
            m_PlayerTypeCreated.PlayerExtra.transform.position = newPosition;
            m_PlayerTypeCreated.PlayerExtra.transform.LookAt(m_Table);
        }
    }
}
