using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [RequireComponent(typeof(Collider))]
    public class SnapDropZoneTag : MonoBehaviour
    {
        public string Label = "all";
        public GameObject Root;

        private void Awake()
        {
            if (Root == null) Root = gameObject;
        }
    }
}
