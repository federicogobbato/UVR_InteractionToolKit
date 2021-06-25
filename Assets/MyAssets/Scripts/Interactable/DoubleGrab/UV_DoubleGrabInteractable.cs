using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    public class UV_DoubleGrabInteractable : XRGrabInteractable
    {
        //Custom event that can be called whenever we need
        public event Action eSecondaryOnHoverExit;
        public event Action ePrimanyOnSelect;
        public event Action ePrimaryOnSelectExit;
        public event Action eSecondaryGrabUpdate; 

        public XRDirectInteractor FirstGrab { get; private set; } = null;
        public XRDirectInteractor SecondaryGrab { get; private set; } = null;
        public Vector3 DirectionGrab { get; protected set; }

        bool m_SecondaryGrabIsHover = false;

        private void Start()
        {
            //The object is detached from the parent when is grabbed...IMPORTANT
            retainTransformParent = false;
        }

        /// <summary>
        /// On touch The Second interactor is registered if there is alredy a First interactor, 
        /// and it can not grab anymore objects until OnHoverExit  
        /// </summary>
        protected override void OnHoverEntering(XRBaseInteractor interactor)
        {
            if (!(interactor is XRDirectInteractor))
            {
                Debug.LogWarning("The interactor must be a XRDirectInteractor (" + gameObject.name + ")");
                return;
            }

            if (FirstGrab && interactor != FirstGrab)
            {
                m_SecondaryGrabIsHover = true;
                SecondaryGrab = (XRDirectInteractor)interactor;
                SecondaryGrab.allowSelect = false;
            }

            Debug.Log(interactor + " HOVER");

            base.OnHoverEntering(interactor);
        }

        /// <summary>
        /// If the FirstGrab is STILL grabbing the gameobject, base.OnHoverExit is not called 
        /// </summary>
        protected override void OnHoverExiting(XRBaseInteractor interactor)
        {
            Debug.Log(interactor + " HOVER EXIT");

            if (FirstGrab && interactor == FirstGrab)
            {
                Debug.Log(interactor + " HOVER EXIT PRIMARY");
                return;
            }
            else if (interactor == SecondaryGrab)
            {
                m_SecondaryGrabIsHover = false;
            }

            base.OnHoverExiting(interactor);
        }


        /// <summary>
        /// 1. The First interactor is registered 
        /// 2. If the gameObject has a parent it's detached from the parent and its rotation is resetted
        /// </summary>
        protected override void OnSelectEntering(XRBaseInteractor interactor)
        {
            if (!(interactor is XRDirectInteractor))
            {
                Debug.LogWarning("The interactor must be a XRDirectInteractor");
                return;
            }

            Debug.Log(interactor + " SELECT ");

            if (!FirstGrab)
            {
                Debug.Log(interactor + " SELECT PRIMARY");

                base.OnSelectEntering(interactor);

                FirstGrab = (XRDirectInteractor)interactor;

                ePrimanyOnSelect?.Invoke();
            }
        }


        /// <summary>
        /// The GameObject is ungrabbed
        /// </summary>
        protected override void OnSelectExiting(XRBaseInteractor interactor)
        {
            if (FirstGrab == interactor)
            {
                Debug.Log(interactor + " SELECT EXIT PRIMARY");

                FirstGrab = null;

                if (SecondaryGrab)
                {
                    m_SecondaryGrabIsHover = false;
                    SecondaryGrab.allowSelect = true;
                    SecondaryGrab = null;
                }

                ePrimaryOnSelectExit?.Invoke();

                base.OnSelectExiting(interactor);
            }
        }


        private void Update()
        {
            //If a secondary object is touching the object and the grab button is pressed something happen....
            if (SecondaryGrab)
            {
                bool secondaryGrabPressed = false;
                InputHelpers.IsPressed(SecondaryGrab.GetComponent<XRController>().inputDevice, SecondaryGrab.GetComponent<XRController>().selectUsage, out secondaryGrabPressed);

                if (secondaryGrabPressed)
                {
                    DirectionGrab = SecondaryGrab.transform.position - FirstGrab.transform.position;

                    eSecondaryGrabUpdate?.Invoke();
                }
                else
                {
                    if (!m_SecondaryGrabIsHover)
                    {
                        eSecondaryOnHoverExit?.Invoke();
                        SecondaryGrab.allowSelect = true;
                        SecondaryGrab = null;
                    }
                }
            }
        }
    }
}
