using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UVR
{
    [RequireComponent(typeof(UV_DoubleGrabInteractable))]
    [DisallowMultipleComponent]
    public class UV_ResizeOnGrab : MonoBehaviour
    {
        [Tooltip("The axes will not be scaled (local axes of the Model)")]
        [SerializeField] protected Vector3State m_ScaledAxis = Vector3State.True;

        [Tooltip("Axis resizing disabled when maxScale is reached")]
        [SerializeField] protected Vector3State m_StopResizeMaxScaleReached = Vector3State.True;

        [SerializeField] protected GameObject m_Model;

        [Space]
        [SerializeField] protected float m_ResizeMultiplier = 1.0f;
        [SerializeField] protected float m_MaxScale = 0.0f;
        [SerializeField] protected float m_MinScale = 0.0f;

        UV_DoubleGrabInteractable m_Interactable;

        bool m_FirstScale = false;
        Vector3 m_StartScale = Vector3.zero;
        float m_FirstDistanceGrab = 0;
        float m_CurrentDistanceGrab = 0;

        Vector3State m_MaxSizeReached = Vector3State.False;

        private void OnEnable()
        {
            m_Interactable = GetComponent<UV_DoubleGrabInteractable>();

            if (m_Interactable)
            {
                m_Interactable.ePrimaryOnSelectExit += ResetVariables;
                m_Interactable.eSecondaryOnHoverExit += ResetVariables;
                m_Interactable.eSecondaryGrabUpdate += Scale;
            }
            else
            {
                Debug.LogError("No interactable attached to the gameObject");
            }
        }

        private void OnDisable()
        {
            if (m_Interactable)
            {
                m_Interactable.ePrimaryOnSelectExit -= ResetVariables;
                m_Interactable.eSecondaryOnHoverExit -= ResetVariables;
                m_Interactable.eSecondaryGrabUpdate -= Scale;
            }
        }

        private void Start()
        {
            if (!m_Model)
            {
                Debug.LogError("No Model found");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }

            Vector3 currentScale = m_Model.transform.localScale;
            List<float> scales = new List<float>();
                
            if (m_ScaledAxis.xState && m_MinScale > currentScale.x) scales.Add(currentScale.x - currentScale.x * 0.01f);
            if (m_ScaledAxis.yState && m_MinScale > currentScale.y) scales.Add(currentScale.y - currentScale.y * 0.01f);
            if (m_ScaledAxis.zState && m_MinScale > currentScale.z) scales.Add(currentScale.z - currentScale.z * 0.01f);

            m_MinScale = Mathf.Min(scales.ToArray());
        }

        private void ResetVariables()
        {
            m_FirstScale = false;
            m_FirstDistanceGrab = 0;
            m_StartScale = Vector3.zero;
            m_CurrentDistanceGrab = 0;

            m_MaxSizeReached = Vector3State.False;
        }

        /// <summary>
        /// Set he spin of the resize based on the distance between the controllers 
        /// </summary>
        void CalculateDistanceGrabs()
        {
            Vector3 localModelFirstPosition = m_Model.transform.InverseTransformDirection(m_Interactable.FirstGrab.transform.position);
            Vector3 localModelSecondPosition = m_Model.transform.InverseTransformDirection(m_Interactable.SecondaryGrab.transform.position);

            m_CurrentDistanceGrab = Vector3.Distance(localModelFirstPosition, localModelSecondPosition);
        }


        private void Scale()
        {
            //////Unparent the children to avoid resizing 
            ////foreach (var obj in objectsNotScaled)
            ////{
            ////    obj.transform.parent = null;
            ////}

            CalculateDistanceGrabs();

            if (!m_FirstScale)
            {
                m_FirstScale = true;
                m_StartScale = m_Model.transform.localScale;
                m_FirstDistanceGrab = m_CurrentDistanceGrab;
            }

            Vector3 existingScale = m_Model.transform.localScale;

            m_MaxSizeReached.xState = existingScale.x >= m_MaxScale ? true : false;
            m_MaxSizeReached.yState = existingScale.y >= m_MaxScale ? true : false;
            m_MaxSizeReached.zState = existingScale.z >= m_MaxScale ? true : false;

            if ((!m_ScaledAxis.xState || (m_MaxSizeReached.xState && m_StopResizeMaxScaleReached.xState)) &&
                (!m_ScaledAxis.yState || (m_MaxSizeReached.yState && m_StopResizeMaxScaleReached.yState)) &&
                (!m_ScaledAxis.zState || (m_MaxSizeReached.zState && m_StopResizeMaxScaleReached.zState)))
            {
                return;
            }

            Vector3 newScale = m_StartScale * (m_CurrentDistanceGrab / m_FirstDistanceGrab) * m_ResizeMultiplier;

            float finalScaleX = !m_ScaledAxis.xState || newScale.x <= m_MinScale || m_MaxSizeReached.xState ? existingScale.x : newScale.x;
            float finalScaleY = !m_ScaledAxis.yState || newScale.y <= m_MinScale || m_MaxSizeReached.yState ? existingScale.y : newScale.y;
            float finalScaleZ = !m_ScaledAxis.zState || newScale.z <= m_MinScale || m_MaxSizeReached.zState ? existingScale.z : newScale.z;

            if (finalScaleX > 0 && finalScaleY > 0 && finalScaleZ > 0)
            {
                m_Model.transform.localScale = new Vector3(finalScaleX, finalScaleY, finalScaleZ);
            }

            ////foreach (var obj in objectsNotScaled)
            ////{
            ////    obj.transform.parent = m_Model.transform;
            ////}
        }
    }
}


