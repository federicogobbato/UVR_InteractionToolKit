
using System;
using UnityEngine;

namespace UVR.NetworkPhoton
{
    [Serializable]
    public class PlayerType
    {
        public PlayerTypeSelector.TypePlayer Type;

        public PUN_BasePlayerManager PlayerPrefab;

        public GameObject PlayerExtra;
    }
}

public class PlayerTypeSelector : Singleton<PlayerTypeSelector>
{
    public enum TypePlayer { VR_OCULUS, VR_CARDBOARD, PC };

    public TypePlayer PlayerTypeChosen;
}