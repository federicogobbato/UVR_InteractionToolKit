using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UVR.NetworkPhoton;

public class PlayerManagerGC : PUN_BasePlayerManager
{
    [SerializeField] Transform m_RemoteHead;

    Transform m_GoogleCardboardCamera;

    private void Start()
    {
        if (photonView.AmOwner)
        {
            m_GoogleCardboardCamera = Camera.main.transform;
        }
    }

    ///<summary>
    /// Update transform of local hands and head
    ///</summary>
    void Update()
    {
        if (photonView.AmOwner)
        {
            m_RemoteHead.position = m_GoogleCardboardCamera.position;
            m_RemoteHead.rotation = m_GoogleCardboardCamera.rotation;
        }
    }
}
