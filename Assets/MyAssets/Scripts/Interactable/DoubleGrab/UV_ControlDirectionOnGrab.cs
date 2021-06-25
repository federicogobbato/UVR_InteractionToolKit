using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [RequireComponent(typeof(UV_DoubleGrabInteractable))]
    [DisallowMultipleComponent]
    public class UV_ControlDirectionOnGrab : MonoBehaviour
    {
        UV_DoubleGrabInteractable m_Interactable;
        GameObject m_Model;
        Vector3 m_ModelRotation;


        private void OnEnable()
        {
            m_Interactable = GetComponent<UV_DoubleGrabInteractable>();

            if (m_Interactable)
            {
                m_Interactable.ePrimanyOnSelect += AllignOnGrab;
                m_Interactable.ePrimaryOnSelectExit += FixRotationOnUngrab;
                m_Interactable.eSecondaryGrabUpdate += UpdateDirectionModel;
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
                m_Interactable.ePrimanyOnSelect -= AllignOnGrab;
                m_Interactable.ePrimaryOnSelectExit -= FixRotationOnUngrab;
                m_Interactable.eSecondaryGrabUpdate -= UpdateDirectionModel;
            }
        }


        private void Start()
        {
            //Find the model inside the attachTransform
            m_Model = m_Interactable.attachTransform.gameObject.transform.Find("Model").gameObject;

            if (!m_Model)
            {
                Debug.LogError("No Model found as children of the attach transform");
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#endif
            }

            //Save the local rotation of the model 
            m_ModelRotation = m_Model.GetComponent<Transform>().localRotation.eulerAngles;
        }


        /// <summary>
        /// Subtract the rotation of the Model to allign the object with the grab direction
        /// </summary>
        private void AllignOnGrab()
        {
            Vector3 objectRotation = m_Interactable.attachTransform.localRotation.eulerAngles;
            objectRotation -= m_ModelRotation;
            m_Interactable.attachTransform.transform.localRotation = Quaternion.Euler(objectRotation);

            Debug.Log("Model rotation: " + m_ModelRotation);

            StartCoroutine(WaitRotation());
        }


        /// <summary>
        /// Wait until the model reach the right rotation, in the meantime the model is disabled to prevent undesired collision with close objects 
        /// </summary>
        private IEnumerator WaitRotation()
        {
            m_Model.SetActive(false);

            yield return new WaitForSeconds(0.1f);

            m_Model.SetActive(true);
        }


        /// <summary>
        /// Set the rotation of the object equal to the rotation of the attachTransform
        /// so we can reset the local rotation of the attachTransform.
        /// This is done because the this.transform doesn't follow the rotation of the attachTransform    
        /// </summary>
        private void FixRotationOnUngrab()
        {   
            transform.rotation = m_Interactable.attachTransform.transform.rotation;
            m_Interactable.attachTransform.localRotation = Quaternion.Euler(Vector3.zero);
        }


        /// <summary>
        /// Update the direction of the attachTransform based on the position of the second grab controller
        /// </summary>
        private void UpdateDirectionModel()
        {
            m_Interactable.attachTransform.transform.rotation = Quaternion.LookRotation(m_Interactable.DirectionGrab, m_Interactable.SecondaryGrab.gameObject.transform.forward);

            //During the rotation the FirstGrab could detect a "wrong" OnTriggerExit so...
            //we have to add the interactable again to the list of the touched interables to prevent wrong behaviours
            //(for example laser activation during double grab)
            List<XRBaseInteractable> listInteractable = new List<XRBaseInteractable>();
            m_Interactable.FirstGrab.GetValidTargets(listInteractable);

            if (!listInteractable.Contains(m_Interactable) && m_Interactable.FirstGrab is UV_XRDirectInteractor)
            {
                Debug.Log("Interactable ADDED to " + m_Interactable.FirstGrab);
                ((UV_XRDirectInteractor)m_Interactable.FirstGrab).ForceTouch(m_Interactable);
            }
        }
    }

}

