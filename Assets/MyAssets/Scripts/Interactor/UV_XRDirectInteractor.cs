
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
        [HideInInspector] public bool IsBusy = false;

        void Update()
        {
            validTargets.RemoveAll(x => x == null);
        }

        /// <summary>
        /// Remove the interactable from the list of the touched interactables
        /// </summary>
        public void ForceUntouch(XRBaseInteractable interactable)
        {
            validTargets.Remove(interactable);
            //Debug.Log("ValidTargets Count = " + ValidTargets.Count);
        }


        /// <summary>
        /// Add the interactable from the list of the touched interactables 
        /// </summary>
        public void ForceTouch(XRBaseInteractable interactable)
        {
            validTargets.Add(interactable);
            //Debug.Log("ValidTargets Count = " + ValidTargets.Count);
        }


        protected new void OnTriggerEnter(Collider col)
        {
            base.OnTriggerEnter(col);

            if(validTargets.Count > 0)
            {
                if(validTargets[validTargets.Count-1] is UV_AdvancedGrabInteractable)
                {
                    ((UV_AdvancedGrabInteractable)validTargets[validTargets.Count - 1]).SetPrecisionGrab(this, col.ClosestPoint(transform.position));     
                }
            }
        }


        protected void OnTriggerStay(Collider col)
        {
            //Search the collider inside the valid targets list
            var interactable = validTargets.Find(inter =>
            {
                if (inter == null) return inter;
                var colliders = inter.GetComponentsInChildren<Collider>().ToList();
                return colliders.Find(c => c == col);
            });

            if (interactable && interactable is UV_AdvancedGrabInteractable)
            {
                ((UV_AdvancedGrabInteractable)interactable).SetPrecisionGrab(this, col.ClosestPoint(transform.position));
            }
        }
    }
}

