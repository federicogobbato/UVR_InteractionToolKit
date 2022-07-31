using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.XR.CoreUtils;
using UVR;
using VRStandardAssets.Utils;

/// <summary>
/// Master script that will handle reading some input on the controller and trigger special events like Teleport or
/// activating the MagicTractorBeam
/// </summary>

[DisallowMultipleComponent]
public class UV_MasterController : Singleton<UV_MasterController>
{
    enum InputType
    {
        MOVE_STICK, TOUCH_STICK, CLICK_BUTTON
    }

    #region PUBLIC FIELDS

    public Camera MainCamera;
    public XRUIInputModule UIInputModule;

    public Transform StartingPosition;

    // TELEPORT
    public XRRayInteractor RightTeleportInteractor;
    public XRRayInteractor LeftTeleportInteractor;
    [SerializeField] InputType m_InputTeleportType = InputType.MOVE_STICK;
    public bool TeleportRightActive = true;
    public bool TeleportLeftActive = true;
    public float TeleportSensibility = 1;
    public GlobalVariables.UVR_BaseEvent OnTeleportFade;

    // LASER
    public XRRayInteractor RightLaserInteractor;
    public XRRayInteractor LeftLaserInteractor;
    public bool RightLaserActive = true;
    public bool LeftLaserActive = true;
    public bool AlwaysActive = false;

    // UI LASER
    public XRRayInteractor RightInteractorUI;
    public XRRayInteractor LeftInteractorUI;
    public bool InteractorUIActive = true;

    // INTERACTORS
    public XRDirectInteractor RightDirectInteractor;
    public XRDirectInteractor LeftDirectInteractor;

    public XRRig Rig { get; private set; }

    [HideInInspector] public bool TouchingPad = false;
    [HideInInspector] public Vector3 TouchDirection;

    [HideInInspector] public InputDevice LeftInputDevice { get; private set; }
    [HideInInspector] public InputDevice RightInputDevice { get; private set; }

    public event Action<XRNode, string, bool> OnPlayPointAnimation;
    public event Action<XRNode, string, bool> OnPlayGrabAnimation;

    #endregion

    #region PRIVATE FIELDS

    EventSystem m_CurrentEventSystem;

    XRInteractorLineVisual m_RightLineVisualTeleport;
    XRInteractorLineVisual m_LeftLineVisualTeleport;
    Vector3 m_PosBeforeTelep = Vector3.zero;

    XRInteractorLineVisual m_RightLineVisualLaser;
    XRInteractorLineVisual m_LeftLineVisualLaser;

    XRInteractorLineVisual m_RightLineVisualUI;
    XRInteractorLineVisual m_LeftLineVisualUI;

    XRController m_RightTelepController;
    XRController m_LeftTelepController;

    XRController m_RightLaserController;
    XRController m_LeftLaserController;

    bool m_LeftIsGripping = false;
    bool m_RightIsGripping = false;

    bool m_IsTeleportingRight = false;
    bool m_IsTeleportingLeft = false;

    private bool m_RightLaserActived = false;
    private bool m_LeftLaserActived = false;

    #endregion


    protected override void Awake()
    {
        base.Awake();
        Rig = GetComponent<XRRig>();

        if (UIInputModule)
        {
            m_CurrentEventSystem = UIInputModule.GetComponent<EventSystem>();
        }
        else
        {
            Debug.LogWarning("UIInputModule is null");
        }
    }

    #region UNITY METHODS

    void OnEnable()
    {
         InputDevices.deviceConnected += RegisterDevice;
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= RegisterDevice;
    }


    void Start()
    {
        FindObjectOfType<LocomotionSystem>().xrRig = GetComponent<XRRig>();

        //** Set Teleport Variables **//
        if(RightTeleportInteractor)
        {
            m_RightLineVisualTeleport = RightTeleportInteractor.GetComponent<XRInteractorLineVisual>();
            RightTeleportInteractor.gameObject.SetActive(false);
            m_RightTelepController = RightTeleportInteractor.GetComponent<XRController>();
        }

        if (LeftTeleportInteractor)
        {
            m_LeftLineVisualTeleport = LeftTeleportInteractor.GetComponent<XRInteractorLineVisual>();
            LeftTeleportInteractor.gameObject.SetActive(false);
            m_LeftTelepController = LeftTeleportInteractor.GetComponent<XRController>();
        }

        //** Set Laser Variables **//
        if (RightLaserInteractor)
        {
            m_RightLineVisualLaser = RightLaserInteractor.GetComponent<XRInteractorLineVisual>();
            m_RightLineVisualLaser.gameObject.SetActive(AlwaysActive);

            m_RightLaserController = RightLaserInteractor.GetComponent<XRController>();

            //Set the max RaycastDistance equal to the maxLength of the line renderer
            RightLaserInteractor.maxRaycastDistance = m_RightLineVisualLaser.lineLength;
        }

        if (LeftLaserInteractor)
        {
            m_LeftLineVisualLaser = LeftLaserInteractor.GetComponent<XRInteractorLineVisual>();
            m_LeftLineVisualLaser.gameObject.SetActive(AlwaysActive);

            m_LeftLaserController = LeftLaserInteractor.GetComponent<XRController>();

            //Set the max RaycastDistance equal to the maxLength of the line renderer
            LeftLaserInteractor.maxRaycastDistance = m_LeftLineVisualLaser.lineLength;
        }

        //** Set laser UI Variables **//
        if (RightInteractorUI)
        {
            m_RightLineVisualUI = RightInteractorUI.GetComponent<XRInteractorLineVisual>();
            m_RightLineVisualUI.enabled = false;

            //Set the max RaycastDistance equal to the maxLength of the line renderer
            RightInteractorUI.maxRaycastDistance = m_RightLineVisualUI.lineLength;
        }

        if (LeftInteractorUI)
        {
            m_LeftLineVisualUI = LeftInteractorUI.GetComponent<XRInteractorLineVisual>();
            m_LeftLineVisualUI.enabled = false;

            //Set the max RaycastDistance equal to the maxLength of the line renderer
            LeftInteractorUI.maxRaycastDistance = m_LeftLineVisualUI.lineLength;
        }

        //** Find the Left and right InputDevice necessary to detect input **//
        RegisterDevices();

        if (StartingPosition)
        {
            transform.position = StartingPosition.position;
            transform.rotation = StartingPosition.rotation;
        }     
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //** Check for grab inputs, only necessary to play the animation on network **//
        AnimateHandGrab();

        //** Check for teleport inputs **//
        if (RightTeleportInteractor && TeleportRightActive && !m_RightLaserActived)
        {
            RightTeleportUpdate();
        }

        if (LeftTeleportInteractor && TeleportLeftActive && !m_LeftLaserActived)
        {
            LeftTeleportUpdate();
        }

        if (m_CurrentEventSystem)
        {
            //** Check if the the controller is raycasting some UI element, to choose wich rayInteractor activate **//
            if (RightInteractorUI && HoverUI(RightInteractorUI) > 0 && InteractorUIActive)
            {
                m_RightLineVisualLaser.gameObject.SetActive(false);
                m_RightLineVisualUI.enabled = true;
            }
            else
            {
                if (m_RightLineVisualUI)
                {
                    m_RightLineVisualUI.enabled = false;
                    m_RightLineVisualUI.reticle.SetActive(false);
                }

                if (m_RightLineVisualLaser && RightLaserActive && !m_IsTeleportingRight)
                    m_RightLaserActived = EnableDisableLaser(RightDirectInteractor, m_RightLaserController, RightInputDevice, m_RightLineVisualLaser);
            }

            //** Check if the the controller is raycasting some UI element, to choose wich rayInteractor activate **//
            if (LeftInteractorUI && HoverUI(LeftInteractorUI) > 0 && InteractorUIActive)
            {
                m_LeftLineVisualLaser.gameObject.SetActive(false);
                m_LeftLineVisualUI.enabled = true;
            }
            else
            {
                if (m_LeftLineVisualUI)
                {
                    m_LeftLineVisualUI.enabled = false;
                    m_LeftLineVisualUI.reticle.SetActive(false);
                }

                if (m_LeftLineVisualLaser && LeftLaserActive && !m_IsTeleportingLeft)
                    m_LeftLaserActived = EnableDisableLaser(LeftDirectInteractor, m_LeftLaserController, LeftInputDevice, m_LeftLineVisualLaser);
            }
        }
    }

    #endregion


    void RegisterDevice(InputDevice connectedDevice)
    {
        if (connectedDevice.isValid)
        {
            if ((connectedDevice.characteristics & InputDeviceCharacteristics.HeldInHand) == InputDeviceCharacteristics.HeldInHand)
            {
                if ((connectedDevice.characteristics & InputDeviceCharacteristics.Left) == InputDeviceCharacteristics.Left)
                {
                    LeftInputDevice = connectedDevice;
                }
                else if ((connectedDevice.characteristics & InputDeviceCharacteristics.Right) == InputDeviceCharacteristics.Right)
                {
                    RightInputDevice = connectedDevice;
                }
            }
        }
    }

    void RegisterDevices()
    {
        //Look for left controller
        InputDeviceCharacteristics leftTrackedControllerFilter = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left;
        List<InputDevice> foundControllers = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(leftTrackedControllerFilter, foundControllers);

        if (foundControllers.Count > 0)
            LeftInputDevice = foundControllers[0];

        //Look for right controller
        InputDeviceCharacteristics rightTrackedControllerFilter = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right;

        InputDevices.GetDevicesWithCharacteristics(rightTrackedControllerFilter, foundControllers);

        if (foundControllers.Count > 0)
            RightInputDevice = foundControllers[0];

    }

    /// <summary>
    /// It use to play the grab animations over a network
    /// </summary>
    void AnimateHandGrab()
    {
        bool gripPressedLeft;
        LeftInputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripPressedLeft);

        if (gripPressedLeft)
        {
            m_LeftIsGripping = true;
            OnPlayGrabAnimation?.Invoke(XRNode.LeftHand, "Selected", true);
        }
        else if (m_LeftIsGripping)
        {
            m_LeftIsGripping = false;
            OnPlayGrabAnimation?.Invoke(XRNode.LeftHand, "Deselected", true);
            OnPlayGrabAnimation?.Invoke(XRNode.LeftHand, "Selected", false);
        }

        bool gripPressedRight;
        RightInputDevice.TryGetFeatureValue(CommonUsages.gripButton, out gripPressedRight);

        if (gripPressedRight)
        {
            m_RightIsGripping = true;
            OnPlayGrabAnimation?.Invoke(XRNode.RightHand, "Selected", true);
        }
        else if (m_RightIsGripping)
        {
            m_RightIsGripping = false;
            OnPlayGrabAnimation?.Invoke(XRNode.RightHand, "Deselected", true);
            OnPlayGrabAnimation?.Invoke(XRNode.RightHand, "Selected", false);
        }
    }


    private bool EnableDisableLaser(XRDirectInteractor directInterector, XRController controller, InputDevice device, XRInteractorLineVisual lineVisual)
    {
        if (!directInterector || !controller || !lineVisual || AlwaysActive) return false;

        bool activePressed = false;

        InputHelpers.IsPressed(device, controller.activateUsage, out activePressed);

        if (activePressed)
        {
            // If the directInteractor is touching some objects the laser can't be activated
            List<XRBaseInteractable> listInteractable = new List<XRBaseInteractable>();
            directInterector.GetValidTargets(listInteractable);

            if (listInteractable.Count > 0)
            {
                activePressed = false;
            }
        }

        lineVisual.gameObject.SetActive(activePressed);
        return activePressed;
    }


    /// <summary>
    /// Detect with how many UI elements the ray is interacting 
    /// </summary>
    int HoverUI(XRRayInteractor rayInteractorUI)
    {
        if (!rayInteractorUI) return 0;

        TrackedDeviceModel model;
        int uiElements = 0;

        if (UIInputModule.GetTrackedDeviceModel(rayInteractorUI, out model))
        {
            TrackedDeviceEventData data = new TrackedDeviceEventData(m_CurrentEventSystem);
            model.CopyTo(data);
            uiElements = data.hovered.Count;
        }
        return uiElements;
    }


    ////bool GetTouchDirection(InputDevice device)
    ////{
    ////    bool touching;
    ////    device.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out touching);

    ////    TouchingPad = touching;

    ////    Vector2 axisInput;
    ////    device.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

    ////    TouchDirection.x = axisInput.x;
    ////    TouchDirection.z = axisInput.y;

    ////    return TouchingPad;
    ////}


    #region TELEPORT 


    void RightTeleportUpdate()
    {
        bool isTeleporting = false;

        if (m_InputTeleportType == InputType.MOVE_STICK)
        {
            Vector2 axisInput;
            RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

            isTeleporting = axisInput.y > 0.5f;
        }
        else if (m_InputTeleportType == InputType.TOUCH_STICK)
        {
            RightInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out isTeleporting);           
        }

        m_IsTeleportingRight = isTeleporting;

        //Enable/Disable the XR Ray Interactor gameobject
        RightTeleportInteractor.gameObject.SetActive(isTeleporting);

        PlayHandAnimation(RightDirectInteractor, "Pointing", isTeleporting);
        OnPlayPointAnimation?.Invoke(XRNode.RightHand, "Pointing", isTeleporting);
    }


    void LeftTeleportUpdate()
    {
        bool isTeleporting = false;

        if (m_InputTeleportType == InputType.MOVE_STICK)
        {
            Vector2 axisInput;
            LeftInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

            isTeleporting = axisInput.y > 0.5f;
        }
        else if (m_InputTeleportType == InputType.TOUCH_STICK)
        {
            LeftInputDevice.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out isTeleporting);
        }

        m_IsTeleportingLeft = isTeleporting;

        //Enable/Disable the XR Ray Interactor gameobject
        LeftTeleportInteractor.gameObject.SetActive(isTeleporting);

        PlayHandAnimation(LeftDirectInteractor, "Pointing", isTeleporting);
        OnPlayPointAnimation?.Invoke(XRNode.LeftHand, "Pointing", isTeleporting);
    }


    /// <summary>
    /// Invoke OnTeleport just if the distance between the controller and point of teleport is bigger than Teleport Sensibility
    /// </summary>
    IEnumerator AfterTeleport(XRController controller)
    {
        m_PosBeforeTelep = transform.position;

        yield return new WaitForSeconds(0.1f);

        if (Mathf.Abs(Vector3.Distance(m_PosBeforeTelep, transform.position)) > TeleportSensibility)
        {
            OnTeleportFade?.Invoke();
        }
    }


    /// <summary>
    /// Enable/Disable telporting for controller
    /// </summary>
    public void SetTeleport(XRController controller, bool state)
    {
        if (!controller) return;

        if (controller.controllerNode == XRNode.LeftHand)
        {
            LeftTeleportInteractor.gameObject.SetActive(state);
            TeleportLeftActive = state;
        }
        else if (controller.controllerNode == XRNode.RightHand)
        {
            RightTeleportInteractor.gameObject.SetActive(state);
            TeleportRightActive = state;
        }
    }

    #endregion


    public void PlayHandAnimation(XRDirectInteractor directInterector, string transiction, bool isActive)
    {
        HandPrefab hand = directInterector.GetComponentInChildren<HandPrefab>();

        if (hand != null)
        {
            hand.Animator.SetBool(transiction, isActive);
        }
    }

    public void PlayHandAnimation(XRDirectInteractor directInterector, string transiction)
    {
        HandPrefab hand = directInterector.GetComponentInChildren<HandPrefab>();

        if (hand != null)
        {
            hand.Animator.SetTrigger(transiction);
        }
    }
}
