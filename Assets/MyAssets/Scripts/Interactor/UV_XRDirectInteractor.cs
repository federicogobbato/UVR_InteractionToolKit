
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    /// <summary>
    /// 1. Can Force touching or untouching interactable
    /// 2. OnTriggerEnter and/or OnTriggerStay if the interectable is precisionGrab, 
    ///    the collision point is passed to interactable
    /// </summary>
    public class UV_XRDirectInteractor : XRDirectInteractor
    {
        void Update()
        {
            List<IXRActivateInteractable> validTargets = new List<IXRActivateInteractable>();
            GetActivateTargets(validTargets);
            validTargets.RemoveAll(x => x == null);
        }

        /// <summary>
        /// Remove the interactable from the list of the touched interactables
        /// </summary>
        public void ForceUntouch(XRBaseInteractable interactable)
        {
            List<IXRActivateInteractable> validTargets = new List<IXRActivateInteractable>();
            GetActivateTargets(validTargets);
            validTargets.Remove(interactable);
            //Debug.Log("ValidTargets Count = " + ValidTargets.Count);
        }


        /// <summary>
        /// Add the interactable to the list of the touched interactables 
        /// </summary>
        public void ForceTouch(XRBaseInteractable interactable)
        {
            List<IXRActivateInteractable> validTargets = new List<IXRActivateInteractable>();
            GetActivateTargets(validTargets);
            validTargets.Add(interactable);
            //Debug.Log("ValidTargets Count = " + ValidTargets.Count);
        }


        protected new void OnTriggerEnter(Collider col)
        {
            base.OnTriggerEnter(col);
            PerformPrecisionGrab(col);
        }


        protected new void OnTriggerStay(Collider col)
        {
            PerformPrecisionGrab(col);
            base.OnTriggerStay(col);
        }

        protected void PerformPrecisionGrab(Collider col)
        {
            var advancedGrab = col.GetComponentInParent<UV_AdvancedGrabInteractable>();
            if (advancedGrab)
            {
                advancedGrab.SetPrecisionGrab(this, col.ClosestPoint(transform.position));
            }
        }

    }
}

