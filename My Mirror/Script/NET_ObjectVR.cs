using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class NET_ObjectVR : NetworkBehaviour
{
    delegate void SetOnServer(bool state);

    public bool Toggle = false;
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    [SyncVar(hook = nameof(HandleGrabChanged))]
    public bool IsGrabbed;
    [SyncVar(hook = nameof(HandleActiveChanged))]
    public bool IsActive;

    public void HandleGrabChanged(bool oldValue, bool newValue) => m_RB.isKinematic = IsGrabbed;
    public void HandleActiveChanged(bool oldValue, bool newValue)
    {
        if (IsActive)
        {
            OnActivated.Invoke();
        }
        else
        {
            OnDeactivated.Invoke();
        }
    }

    Rigidbody m_RB;
    XRGrabInteractable m_GrabInteractable;
    NET_GamePlayer m_LocalPlayer;


    private void Start()
    {
        m_RB = GetComponent<Rigidbody>();
        m_GrabInteractable = GetComponent<XRGrabInteractable>();
    }


    public void SetIsGrabbed(bool value)
    {
        StartCoroutine(SetOwner(value, SetIsGrabbedOnServer));
    }

    public void SetIsActive(bool value)
    {
        StartCoroutine(SetOwner(value, SetIsActiveOnServer));
    }


    /// <summary>
    /// Find the local player checking his authority
    /// </summary>
    private NET_GamePlayer GetLocalPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var pl in players)
        {
            NET_GamePlayer netPlayer = pl.GetComponent<NET_GamePlayer>();
            if (netPlayer && netPlayer.hasAuthority)
            {
                return netPlayer;
            }
        }
        return null;
    }


    /// <summary>
    /// grabbedState = true 
    /// First we set the authority for this object, 
    /// and just when the object has an authority we set the changed variable on the server; 
    /// grabbedstate = false
    /// First we set the changed variable on the server 
    /// and later we remove the authority from this object.
    /// </summary>
    private IEnumerator SetOwner(bool value, SetOnServer setOnServer)
    {
        if(!m_LocalPlayer) m_LocalPlayer = GetLocalPlayer();

        if (m_LocalPlayer)
        {
            if (value)
            {
                m_LocalPlayer.SetOwnerToObject(GetComponent<NetworkIdentity>(), m_LocalPlayer.GetComponent<NetworkIdentity>());
            }
            else
            {
                //Set the variable before remove the authority
                setOnServer(value);
                yield return null;
                m_LocalPlayer.RemoveOwnerToObejct(GetComponent<NetworkIdentity>());
            }
        }

        while (!hasAuthority)
        {
            yield return null;
        }

        //Set the variable on server
        setOnServer(value);
    }


    [Command]
    private void SetIsGrabbedOnServer(bool value)
    {
        IsGrabbed = value;
    }

    [Command]
    private void SetIsActiveOnServer(bool value)
    {
        IsActive = value;
    }
}
