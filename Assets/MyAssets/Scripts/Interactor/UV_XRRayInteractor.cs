using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    public class UV_XRRayInteractor : XRRayInteractor
    {
        public List<XRBaseInteractable> GetValidTargets()
        {           
            return validTargets;
        }
    }
}

