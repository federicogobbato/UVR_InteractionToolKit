using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    public class UV_AngleDragInteractable : XRSimpleInteractable
    {
        [SerializeField] float m_ForceMultiplier = 1000;
        [SerializeField] Rigidbody m_ParentToRotate = null;
        [SerializeField] bool m_UsePhysics = false;

        XRBaseInteractor m_GrabbingInteractor;
        Vector3 m_PreviousDoorPivotToHand;
        bool m_HoldingHandle;

        private void Start()
        {
            if (!m_ParentToRotate) m_ParentToRotate = GetComponentInParent<Rigidbody>();
        }

        protected override void OnSelectEntering(XRBaseInteractor interactor)
        {
            base.OnSelectEntering(interactor);
            m_GrabbingInteractor = interactor;
        }

        protected override void OnSelectExiting(XRBaseInteractor interactor)
        {
            // Set angular velocity to zero if the hand stops interaction
            m_ParentToRotate.angularVelocity = Vector3.zero;

            base.OnSelectExiting(interactor);
        }

        private void Update()
        {
            if (isSelected && !m_UsePhysics)
            {
                Vector3 doorPivotToHand = m_GrabbingInteractor.transform.position - transform.parent.position;
                doorPivotToHand.y = 0;

                Vector3 handleToHand = m_GrabbingInteractor.transform.position - transform.position;
                handleToHand.y = 0;

                if (m_PreviousDoorPivotToHand != Vector3.zero)
                {
                    // Cross product between handleToHand and doorPivotToHand. 
                    Vector3 cross = Vector3.Cross(doorPivotToHand, handleToHand);

                    transform.parent.Rotate(cross, Vector3.Angle(doorPivotToHand, m_PreviousDoorPivotToHand));
                }

                m_PreviousDoorPivotToHand = doorPivotToHand;
            }
        }

        private void FixedUpdate()
        {
            if (isSelected && m_UsePhysics)
            {
                // Direction vector from door handle to hand's current position
                Vector3 handleToHand = m_GrabbingInteractor.transform.position - transform.position;

                handleToHand.y = 0;

                //Apply a force to the door at the position of the handle
                m_ParentToRotate.AddForceAtPosition(handleToHand * m_ForceMultiplier, transform.position);
            }
        }
    }
}

