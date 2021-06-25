using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NET_NoVRPlayer : NET_GamePlayer
{
    public override void OnStartAuthority()
    {
        SetVR();
    }

    void SetVR()
    {
        GameObject.Find("NO VR").SetActive(true);
        GameObject.Find("NET VR").SetActive(false);
    }
}