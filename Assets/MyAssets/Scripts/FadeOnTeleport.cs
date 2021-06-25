using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRStandardAssets.Utils;

public class FadeOnTeleport : MonoBehaviour
{
    public bool Enabled = true;

    [SerializeField] UIFader m_Fader;

    private void OnEnable()
    {
        if (Enabled)
        {
            UV_MasterController.Instance.OnTeleportFade.AddListener(PlayFade);
        }
    }

    private void OnDisable()
    {
        if (Enabled)
        {
            UV_MasterController.Instance.OnTeleportFade.RemoveListener(PlayFade);
        }
    }

    private void PlayFade()
    {
        if (!m_Fader || m_Fader.Fading) return;

        foreach (var canvasGroup in m_Fader.GroupsToFade)
        {
            canvasGroup.alpha = 1;
        }
        Debug.Log("Start Fade");

        StartCoroutine(m_Fader.FadeOut());
    }
}
