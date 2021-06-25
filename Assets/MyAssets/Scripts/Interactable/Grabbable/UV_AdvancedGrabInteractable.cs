using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using static UVR.GlobalVariables;

namespace UVR
{
    /// <summary>
    /// OFFSET: Variation of the classic XRGrabInteractable that will keep the position and rotation offset between the
    /// grabbed object and the controller instead of snapping the object to the controller. 
    /// The attachTransform of the in Interactor is moved at the same position of the attachTransform of the Interactable.
    /// (Better for UX and the illusion of holding the thing (see Tomato Presence : https://owlchemylabs.com/tomatopresence/))
    /// PRECISION_GRAB: Variation of the classic XRGrabInteractable that will move the attachPosition on the current 
    /// hit point of the controller collider with the grabbed object collider.
    /// </summary>
    /// 

    public class UV_AdvancedGrabInteractable : XRGrabInteractable
    {
        public enum ADVANCED_GRAB
        {
            OFFSET, PRECISION_GRAB, NONE
        }

        class SavedTransform
        {
            public Vector3 OriginalPosition;
            public Quaternion OriginalRotation;
        }

        [SerializeField] ADVANCED_GRAB m_AdvancedGrab = ADVANCED_GRAB.OFFSET;
        public ADVANCED_GRAB AdvancedGrab { get { return m_AdvancedGrab; } set { m_AdvancedGrab = value; } }


        #region INSPECTOR FIELDS

        //Custom event that can be called whenever we need
        public event Action eOnHover;
        public event Action eOnHoverExit;
        public event Action eOnSelect;
        public event Action eOnSelectExit;
        public event Action eOnActivate;
        public event Action eOnDeactivate;

        [SerializeField] XRNodeEvo m_TouchableBy = XRNodeEvo.Everything;
        [SerializeField] XRNodeEvo m_GrabbableBy = XRNodeEvo.Everything;
        [SerializeField] XRNodeEvo m_ActivableBy = XRNodeEvo.Everything;

        [Tooltip("Can be activated by a normal interactor or a XRRayInteractor, just touching")]
        [SerializeField] bool m_ActiveOnTouch = false;
        public bool ActiveOnTouch { get { return m_ActiveOnTouch; } set { m_ActiveOnTouch = value; } }

        [SerializeField] bool m_HoldButtonToGrab = true;
        public bool HoldButtonToGrab { get { return m_HoldButtonToGrab; } set { m_HoldButtonToGrab = value; } }

        [SerializeField] bool m_HoldButtonToActivate = true;
        public bool HoldButtonToActivate { get { return HoldButtonToActivate; } set { HoldButtonToActivate = value; } }

        // !DEPRECATED
        ////[Tooltip("Interactable by a XRRayInteractor")]
        ////[SerializeField] bool m_LaserInteractable = true;
        ////public bool LaserInteractable { get { return m_LaserInteractable; } set { m_LaserInteractable = value; } }

        #endregion

        #region FIELDS

        /// <summary>
        /// Interactor and controlller is touching
        /// </summary>
        public XRBaseInteractor CurrentInteractor { get; private set; }
        public XRController CurrentController { get; private set; }

        public bool IsActive { get; private set; } = false;
        public bool IsActiveOnTouch { get; private set; } = false;
        public bool IsGrabbed { get; private set; } = false;

        [HideInInspector] public bool CantHover = false;
        [HideInInspector] public bool CantHoverExit = false;
        [HideInInspector] public bool CantSelect = false;
        [HideInInspector] public bool CantSelectExit = false;
        [HideInInspector] public bool CantActivated = false;
        [HideInInspector] public bool CantDeactivated = false;

        /// <summary>
        /// Used to save the attachTransform of the grabbing interactors
        /// </summary>
        Dictionary<XRBaseInteractor, SavedTransform> m_SavedTransforms = new Dictionary<XRBaseInteractor, SavedTransform>();

        /// <summary>
        /// The Interactors that are touching the object and the current grab precicion point 
        /// </summary>
        Dictionary<UV_XRDirectInteractor, Transform> m_InteractorAttach = new Dictionary<UV_XRDirectInteractor, Transform>();

        protected Rigidbody m_Rb;

        #endregion

        protected override void Awake()
        {
            base.Awake();
            // The base class already grab it but don't expose it so have to grab it again
            m_Rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            if (m_TouchableBy != XRNodeEvo.Everything)
            {
                m_GrabbableBy = m_TouchableBy;
                m_ActivableBy = m_TouchableBy;
            }

            if (m_GrabbableBy != XRNodeEvo.Everything)
            {
                m_ActivableBy = m_GrabbableBy;
            }

            if(!HoldButtonToGrab && AdvancedGrab == ADVANCED_GRAB.OFFSET)
            {
                Debug.LogWarning("<color=red>If HoldButtonToGrab is false the Advanced grab should be Precision Grab or None!<color=red>");
            }
        }

        /// <summary>
        /// Check if the activate button is pressed 
        /// </summary>
        private void Update()
        {
            if (CurrentInteractor && (m_ActiveOnTouch || IsGrabbed))
            {
                bool activeIsPressed = false;
                InputHelpers.IsPressed(CurrentController.inputDevice, CurrentController.activateUsage, out activeIsPressed);

                if (activeIsPressed)
                {
                    if (!IsActiveOnTouch)
                    {
                        OnActivate(CurrentInteractor);
                        IsActiveOnTouch = true;
                    }
                }
                else if (IsActiveOnTouch)
                {
                    eOnDeactivate?.Invoke();
                    base.OnDeactivate(CurrentInteractor);
                    IsActiveOnTouch = false;
                }
            }
        }

        #region INHERITED METHODS

        /// <summary>
        /// The Interactor is registerd 
        /// </summary>
        protected override void OnHoverEntering(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_TouchableBy) || CantHover /*|| (!m_LaserInteractable && interactor is XRRayInteractor)*/) return;

            base.OnHoverEntering(interactor);

            CurrentInteractor = interactor;
            CurrentController = interactor.GetComponent<XRController>();

            eOnHover?.Invoke();
        }

        protected override void OnHoverExiting(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_TouchableBy) || CantHoverExit /*|| (!m_LaserInteractable && interactor is XRRayInteractor)*/) return;

            if (m_ActiveOnTouch)
            {
                eOnDeactivate?.Invoke();
                base.OnDeactivate(interactor);
            }
            ResetCurrentTouchInteractor();

            eOnHoverExit?.Invoke();

            base.OnHoverExiting(interactor);
        }

        protected override void OnSelectEntering(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_GrabbableBy) || CantSelect) return;

            ResetCurrentTouchInteractor();
            if (m_ActiveOnTouch)
                base.OnDeactivate(CurrentInteractor);

            if (!m_HoldButtonToGrab)
            {
                if (!IsGrabbed)
                {
                    ProcessGrab(interactor);

                    //Disable model(hand) controller animations and invert grab animation 
                    ////interactor.GetComponent<XRController>().animateModel = false;
                    ////interactor.GetComponent<XRController>().modelSelectTransition = "Deselected";

                    eOnSelect?.Invoke();
                    IsGrabbed = true;
                }
                else
                {
                    IsGrabbed = false;
                    eOnSelectExit?.Invoke();

                    UnGrab(interactor);

                    //Enable model(hand) controller animations and reset grab animation 
                    ////interactor.GetComponent<XRController>().animateModel = true;
                    ////interactor.GetComponent<XRController>().modelSelectTransition = "Selected";
                }

                CurrentInteractor = interactor;
                CurrentController = interactor.GetComponent<XRController>();
            }
            else
            {
                ProcessGrab(interactor);

                eOnSelect?.Invoke();
            }
        }


        protected override void OnSelectExiting(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_GrabbableBy) || CantSelectExit) return;

            if (m_HoldButtonToGrab)
            {
                eOnSelectExit?.Invoke();

                UnGrab(interactor);

                CurrentInteractor = interactor;
                CurrentController = interactor.GetComponent<XRController>();
            }
        }


        protected override void OnActivate(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_ActivableBy) || CantActivated) return;

            if (!m_HoldButtonToActivate)
            {
                IsActive = !IsActive;
                if (IsActive)
                {
                    base.OnActivate(interactor);
                }
                else
                {
                    eOnDeactivate?.Invoke();
                    base.OnDeactivate(interactor);
                }
            }
            else
            {
                base.OnActivate(interactor);
            }

            eOnActivate?.Invoke();
        }


        protected override void OnDeactivate(XRBaseInteractor interactor)
        {
            if (!IsRightInteractor(interactor, m_ActivableBy) || CantDeactivated) return;

            if (m_HoldButtonToActivate)
            {
                eOnDeactivate?.Invoke();
                base.OnDeactivate(interactor);
            }
        }

        #endregion



        protected void ProcessGrab(XRBaseInteractor interactor)
        {
            if (interactor is XRDirectInteractor && m_AdvancedGrab != ADVANCED_GRAB.NONE)
            {
                SaveInteractorAttach(interactor);

                if(m_AdvancedGrab == ADVANCED_GRAB.OFFSET)
                {
                    bool haveAttach = attachTransform != null;

                    Vector3 offset = Vector3.zero;
					
					// !Better use PRECISION_GRAB or NONE for SnapDropZone objects
                    ////if (transform.parent && transform.parent.GetComponent<SnapDropZone>())
                    ////    offset = m_Rb.centerOfMass;

                    // The position and rotation of the interactor attachTransform are modified
                    interactor.attachTransform.position = haveAttach ? attachTransform.position : m_Rb.worldCenterOfMass /*- offset*/;
                    interactor.attachTransform.rotation = haveAttach ? attachTransform.rotation : m_Rb.rotation;
                }
                else if(m_AdvancedGrab == ADVANCED_GRAB.PRECISION_GRAB &&
                        interactor is UV_XRDirectInteractor)
                {
                    attachTransform = m_InteractorAttach[interactor as UV_XRDirectInteractor];
                    // Just the rotation of the interactor attachTransform is modified
                    interactor.attachTransform.rotation = attachTransform.rotation;
                }
            }

            base.OnSelectEntering(interactor);
        }


        protected void UnGrab(XRBaseInteractor interactor)
        {
            if (interactor is XRDirectInteractor && 
                m_AdvancedGrab != ADVANCED_GRAB.NONE)
            {
                RemoveInteractorAttach(interactor);
            }

            base.OnSelectExiting(interactor);
        }


        protected void SaveInteractorAttach(XRBaseInteractor interactor)
        {
            SavedTransform savedTransform = new SavedTransform();

            savedTransform.OriginalPosition = interactor.attachTransform.localPosition;
            savedTransform.OriginalRotation = interactor.attachTransform.localRotation;

            m_SavedTransforms[interactor] = savedTransform;
        }


        protected void RemoveInteractorAttach(XRBaseInteractor interactor)
        {
            SavedTransform savedTransform = null;
            if (m_SavedTransforms.TryGetValue(interactor, out savedTransform))
            {
                interactor.attachTransform.localPosition = savedTransform.OriginalPosition;
                interactor.attachTransform.localRotation = savedTransform.OriginalRotation;

                m_SavedTransforms.Remove(interactor);
            }
        }

        /// <summary>
        /// Move the attachTrasform (..and create one if it's null) to the newPosition 
        /// </summary>
        /// <param name="interactor">Interactor is touching the interactable</param>
        /// <param name="newPosition">The collision point between the Intreactor and the Interactable</param>
        public void SetPrecisionGrab(UV_XRDirectInteractor interactor, Vector3 newPosition)
        {
            if (m_AdvancedGrab == ADVANCED_GRAB.PRECISION_GRAB)
            {
                if (!m_InteractorAttach.ContainsKey(interactor))
                {
                    GameObject attach = new GameObject("Attach " + interactor);
                    attach.transform.parent = this.transform;
                    attachTransform = attach.transform;

                    m_InteractorAttach[interactor] = attachTransform;
                }
                else
                {
                    attachTransform = m_InteractorAttach[interactor];
                    if (!attachTransform)
                    {
                        RemoveInteractorAttach(interactor);
                        return;
                    }
                }

                attachTransform.position = newPosition;
                attachTransform.rotation = m_Rb.rotation;
            }
        }


        public void ResetCurrentTouchInteractor()
        {
            if (CurrentInteractor && IsActiveOnTouch)
            {
                IsActiveOnTouch = false;
            }

            CurrentInteractor = null;
            CurrentController = null;
        }


        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            int interactorLayerMask = 1 << interactor.gameObject.layer;
            return base.IsSelectableBy(interactor) && (interactionLayerMask.value & interactorLayerMask) != 0;
        }


        /// <summary>
        /// Check if the touching, grabbing or activating controller is the right one
        /// </summary>
        private bool IsRightInteractor(XRBaseInteractor interactor, XRNodeEvo rightInteractor)
        {
            if (interactor == null) return false;

            return rightInteractor == XRNodeEvo.Everything ||
               (int)rightInteractor == (int)interactor.gameObject.GetComponent<XRController>().controllerNode;
        }
    }
}