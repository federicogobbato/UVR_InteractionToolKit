using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using System;

public class NET_VRPlayer : NET_GamePlayer
{
    // GameObjects that must be shown to others client
    [SerializeField] Transform RemoteHead;
    [SerializeField] Transform RemoteHandLeft;
    [SerializeField] Transform RemoteHandRight;
    [SerializeField] NetworkAnimator AnimatorRightHand;
    [SerializeField] NetworkAnimator AnimatorLeftHand;

    // GameObjects used to track position and rotation of the remote ones
    Transform VRCamera;
    Transform LeftHand;
    Transform RightHand;

    bool m_IsInitialized = false;

    public override void OnStartAuthority()
    {
        SetVR();
        Init();
    }

    void SetVR()
    {
        GameObject.Find("NO VR").SetActive(false);
        GameObject.Find("NET VR").SetActive(true);
    }

    public void Init()
    {
        UV_MasterController masterController = UV_MasterController.Instance;

        masterController.OnPlayPointAnimation += PlayHandAnimation;
        masterController.OnPlayGrabAnimation += PlayTriggerHandAnimation;

        VRCamera = masterController.MainCamera.transform;
        LeftHand = masterController.LeftDirectInteractor.gameObject.transform;
        RightHand = masterController.RightDirectInteractor.gameObject.transform;

        m_IsInitialized = true;
    }

    private void OnDisable()
    {
        if (hasAuthority)
        {
            UV_MasterController.Instance.OnPlayPointAnimation -= PlayHandAnimation;
            UV_MasterController.Instance.OnPlayGrabAnimation -= PlayTriggerHandAnimation;
        }
    }


    private void Update()
    {
        if (m_IsInitialized && hasAuthority)
        {
            UpdateTransform();
        }  
    }

    private void UpdateTransform()
    {
        RemoteHead.position = VRCamera.position;
        RemoteHead.rotation = VRCamera.rotation;

        RemoteHandLeft.position = LeftHand.position;
        RemoteHandLeft.rotation = LeftHand.rotation;

        RemoteHandRight.position = RightHand.position;
        RemoteHandRight.rotation = RightHand.rotation;
    }


    /// <summary>
    /// Allow to play other animations not controllled by XRController, as the Pointing animation before a teleport 
    /// </summary>
    /// <param name="handNode"></param>
    /// <param name="transiction"></param>
    /// <param name="state"></param>
    public void PlayHandAnimation(XRNode handNode, string transiction, bool state)
    {
        if (hasAuthority)
        {
            if (handNode == XRNode.RightHand)
                AnimatorRightHand.animator.SetBool(transiction, state);
            else if (handNode == XRNode.LeftHand)
                AnimatorLeftHand.animator.SetBool(transiction, state);
        }
    }

    private void PlayTriggerHandAnimation(XRNode handNode, string transiction, bool reset)
    {
        if (hasAuthority)
        {
            if (handNode == XRNode.RightHand)
            {
                if (!reset)
                    AnimatorRightHand.SetTrigger(transiction);
                else
                    AnimatorRightHand.ResetTrigger(transiction);
            }
            else if (handNode == XRNode.LeftHand)
            {
                if (!reset)
                    AnimatorLeftHand.SetTrigger(transiction);
                else
                    AnimatorLeftHand.ResetTrigger(transiction);
            }
        }        
    }
}
