
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Management;

/// <summary>
/// Disable/Anable VR
/// </summary>
public class ActivatorVR : Singleton<ActivatorVR>
{
    public bool m_isVR = true; 

    [SerializeField] Button ButtonEnableVR;

    void Start()
    {
        if(!m_isVR)
            StopXR();

        if (ButtonEnableVR)
        {
            ButtonEnableVR.image.color = m_isVR ? Color.green : Color.red;
        }
    }

    public void StartXR()
    {
        StartCoroutine(StartXRCoroutine());
    }

    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
            m_isVR = false;
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
            m_isVR = true;
        }
    }

    public void StopXR()
    {
        Debug.Log("Stopping XR...");
        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
}

