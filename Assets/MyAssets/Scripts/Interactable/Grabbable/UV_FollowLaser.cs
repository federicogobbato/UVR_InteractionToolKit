using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [RequireComponent(typeof(UV_AdvancedGrabInteractable))]
    [DisallowMultipleComponent]
    public class UV_FollowLaser : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] MeshRenderer m_MainMeshRenderer;
        [SerializeField] bool m_RotationEnabled = false;
        [SerializeField] float m_TrackSpeed = 3;
        [SerializeField] float m_MinDistanceWithController = 0.1f;

        /// <summary>
        /// The hit point position of the touching laser.
        /// </summary>
        Vector3 m_NewPositionInteractable;

        /// <summary>
        /// The movement applied at the end of a frame to prevent compenetration with other geometries.
        /// It's half the size of the Mesh associated with the MainMeshRenderer.
        /// </summary>
        Vector3 m_Offset = Vector3.zero;

        Rigidbody m_RB = null;
        UV_AdvancedGrabInteractable m_Interactable = null;
        XRRayInteractor m_RayInteractor;
        XRInteractorLineVisual m_LineVisual;

        float m_RayCastMaxDistance;
        float m_LaserLenghtAtBegin;

        bool m_LaserIsHover = false;
        bool m_IsPulling = false;

        #endregion

        private void OnEnable()
        {
            m_Interactable = GetComponent<UV_AdvancedGrabInteractable>();

            if (m_Interactable)
            {
                m_Interactable.eOnHover += LaserHover;
                m_Interactable.eOnDeactivate += LaserDeactivated;

                if(!m_Interactable.IsActivedOnTouch)
                {
                    Debug.LogWarning("<color=red>Active on Touch must be set to true on UV_AdvancedGrabInteractable</color>");
                }
            }
            else
            {
                Debug.LogError("No interactable attached to the gameObject");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }
        }


        private void OnDisable()
        {
            if (m_Interactable)
            {
                m_Interactable.eOnHover -= LaserHover;
                m_Interactable.eOnDeactivate -= LaserDeactivated;
            }
        }


        private void Start()
        {
            m_RB = GetComponent<Rigidbody>();

            if (!m_MainMeshRenderer)
                m_MainMeshRenderer = GetComponent<MeshRenderer>();

            if (!m_MainMeshRenderer)
            {
                Debug.LogError(gameObject + ": no MainMeshRenderer found");
                return;
            }

            m_Offset = m_MainMeshRenderer.bounds.size;

            m_Offset.x /= 2;
            m_Offset.y /= 2;
            m_Offset.z /= 2;
        }


        private void LaserHover()
        {
            if (m_Interactable.CurrentInteractor is XRRayInteractor)
            {
                //to prevent that the laser/ray hit the interactable
                gameObject.layer = LayerMask.NameToLayer("NoLaserInteraction");
                m_RB.isKinematic = true;
                m_RB.constraints = RigidbodyConstraints.FreezeRotation; 
                m_Interactable.CantHover = true;
                m_Interactable.CantHoverExit = true;
                m_Interactable.CurrentInteractor.allowHover = false;

                m_LineVisual = m_Interactable.CurrentInteractor.GetComponent<XRInteractorLineVisual>();
                m_LaserLenghtAtBegin = m_LineVisual.lineLength;

                m_RayInteractor = m_Interactable.CurrentController.GetComponent<XRRayInteractor>();
                m_RayCastMaxDistance = m_RayInteractor.maxRaycastDistance;

                //Disable/Enable the teleport because the touchpad is used for teleporting
                if (m_RotationEnabled)
                    UV_MasterController.Instance.SetTeleport(m_Interactable.CurrentController, false);

                m_LaserIsHover = true;
            }
        }
   

        private void LaserDeactivated()
        {
            if (m_Interactable.CurrentInteractor is XRRayInteractor)
            {
                m_LaserIsHover = false;

                gameObject.layer = LayerMask.NameToLayer("Default");
                m_RB.isKinematic = false;
                m_RB.constraints = RigidbodyConstraints.None;
                m_Interactable.CantHover = false;
                m_Interactable.CantHoverExit = false;
                m_Interactable.CurrentInteractor.allowHover = true;

                m_LineVisual.lineLength = m_LaserLenghtAtBegin;
                m_LineVisual = null;

                m_RayInteractor.maxRaycastDistance = m_RayCastMaxDistance;
                m_RayInteractor = null;

                //Disable/Enable the teleport because the touchpad is used for teleporting
                if (m_RotationEnabled)
                    UV_MasterController.Instance.SetTeleport(m_Interactable.CurrentController, true);

                m_Interactable.ResetCurrentTouchInteractor();
            }
        }


        /// <summary>
        /// Check if the grip button is pressed, used to pull the object close to the controller
        /// </summary>
        void IsPulling()
        {
            if (!m_LaserIsHover) return;

            bool gripPressed = false;
            InputHelpers.IsPressed(m_Interactable.CurrentController.inputDevice, InputHelpers.Button.Grip, out gripPressed);

            if (gripPressed)
            {
                if (m_LineVisual.lineLength > m_MinDistanceWithController + m_Offset.magnitude)
                {
                    m_RayInteractor.maxRaycastDistance = Vector3.Distance(m_LineVisual.transform.position, transform.position);
                    m_IsPulling = true;
                }
                else
                {
                    m_IsPulling = false;
                }
            }
            else
            {
                //The raycast leght is reset
                m_RayInteractor.maxRaycastDistance = m_RayCastMaxDistance;
                m_IsPulling = false;
            }

        }


        /// <summary>
        /// Define the next position of the interactable using the laser/hit position and the offset.
        /// Just later, its rigidbody is uded to move the interactable
        /// </summary>
        private void Update()
        {
            if (m_Interactable.IsActivedOnTouch && 
                m_RayInteractor)
            {
                IsPulling();

                if (m_IsPulling)
                {
                    m_LineVisual.lineLength -= m_TrackSpeed * Time.deltaTime;
                    SetPositionInteractable();
                }
                else 
                {
                    RaycastHit hit;
                    if (m_RayInteractor.TryGetCurrent3DRaycastHit(out hit))
                    {
                        if (hit.collider)
                        {
                            //If the laser collid an object in the scene the interactable is moved at the collision position
                            m_NewPositionInteractable = hit.point;
                            m_LineVisual.lineLength = Vector3.Distance(m_LineVisual.transform.position, transform.position);

                            m_NewPositionInteractable.x += m_Offset.x * hit.normal.x;
                            m_NewPositionInteractable.y += m_Offset.y * hit.normal.y;
                            m_NewPositionInteractable.z += m_Offset.z * hit.normal.z;
                        }
                    }
                    else
                    {
                        //If the laser doesn't collid anythig is moved at the top of the laser
                        SetPositionInteractable();
                    }
                }

                //Update the to rotation of the object 
                if (m_RotationEnabled && UV_MasterController.Instance.TouchingPad)
                {
                    Vector3 rotationProjected = Vector3.ProjectOnPlane(UV_MasterController.Instance.TouchDirection, gameObject.transform.up);
                    Quaternion rotationLookAt = Quaternion.LookRotation(rotationProjected, gameObject.transform.up);
                    gameObject.transform.rotation = rotationLookAt;
                }
            }
        }


        /// <summary>
        /// Set the position of the object at the top of the laser
        /// </summary>
        void SetPositionInteractable()
        {
            //We get the last vertex of the line renderer 
            LineRenderer lineRenderer = m_LineVisual.GetComponent<LineRenderer>();
            Vector3[] vertexLR = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(vertexLR);

            m_NewPositionInteractable = vertexLR[lineRenderer.positionCount - 1];
        }


        private void FixedUpdate()
        {
            if (m_Interactable.IsActivedOnTouch && m_Interactable.CurrentInteractor is XRRayInteractor && m_RB)
            {
                m_RB.position = m_NewPositionInteractable;
            }
        }
    }
}


