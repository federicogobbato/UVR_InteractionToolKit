using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UVR
{
    [Serializable]
    public class ButtonPressed : UnityEvent<bool> { }

    [Serializable]
    public class AxisTouched : UnityEvent<Vector2> { }

    public class UV_InputDisplay : Singleton<UV_InputDisplay>
    {
        public ButtonPressed GripPressedLeft;
        public ButtonPressed TriggerPressedLeft;
        public ButtonPressed PrimaryButtonPressedLeft;
        public ButtonPressed SecondaryButtonPressedLeft;
        public ButtonPressed Primary2DAxisClickedLeft;
        public AxisTouched Primary2DAxisTouchedLeft;

        public ButtonPressed GripPressedRight;
        public ButtonPressed TriggerPressedRight;
        public ButtonPressed PrimaryButtonPressedRight;
        public ButtonPressed SecondaryButtonPressedRight;
        public ButtonPressed Primary2DAxisClickedRight;
        public AxisTouched Primary2DAxisTouchedRight;

        [SerializeField]
        List<InputHelpers.Button> m_InputToObserve = new List<InputHelpers.Button>();

        [HideInInspector] public bool GripL = false;
        [HideInInspector] public bool TriggerL = false;
        [HideInInspector] public bool PrimaryButtonL = false;
        [HideInInspector] public bool SecondaryButtonL = false;
        [HideInInspector] public bool Primary2DAxisTouchedL = false;
        [HideInInspector] public bool Primary2DAxisClickedL = false;

        [HideInInspector] public bool GripR = false;
        [HideInInspector] public bool TriggerR = false;
        [HideInInspector] public bool PrimaryButtonR = false;
        [HideInInspector] public bool SecondaryButtonR = false;
        [HideInInspector] public bool Primary2DAxisTouchedR = false;
        [HideInInspector] public bool Primary2DAxisClickedR = false;

        private void Update()
        {
            bool buttonRightPressed;
            bool buttonLeftPressed;

            InputDevice rightDevice = UV_MasterController.Instance.RightInputDevice;
            InputDevice leftDevice = UV_MasterController.Instance.LeftInputDevice;

            foreach (var button in m_InputToObserve)
            {
                InputHelpers.IsPressed(rightDevice, button, out buttonRightPressed);
                InputHelpers.IsPressed(leftDevice, button, out buttonLeftPressed);

                switch (button)
                {
                    case InputHelpers.Button.Grip:       
                        GripL = buttonLeftPressed;
                        GripR = buttonRightPressed;

                        GripPressedLeft?.Invoke(GripL);
                        GripPressedRight?.Invoke(GripR);
                        break;

                    case InputHelpers.Button.Trigger:   
                        TriggerL = buttonLeftPressed;
                        TriggerR = buttonRightPressed;

                        TriggerPressedLeft?.Invoke(TriggerL);
                        TriggerPressedRight?.Invoke(TriggerR);
                        break;

                    case InputHelpers.Button.PrimaryButton:
                        PrimaryButtonL = buttonLeftPressed;
                        PrimaryButtonR = buttonRightPressed;

                        PrimaryButtonPressedLeft?.Invoke(PrimaryButtonL);
                        PrimaryButtonPressedRight?.Invoke(PrimaryButtonR);
                        break;

                    case InputHelpers.Button.SecondaryButton:
                        SecondaryButtonL = buttonLeftPressed;
                        SecondaryButtonR = buttonRightPressed;

                        SecondaryButtonPressedLeft?.Invoke(SecondaryButtonL);
                        SecondaryButtonPressedRight?.Invoke(SecondaryButtonR);
                        break;

                    case InputHelpers.Button.Primary2DAxisTouch:
                        Primary2DAxisTouchedL = buttonLeftPressed;
                        Primary2DAxisTouchedR = buttonRightPressed;

                        if (buttonRightPressed)
                        {
                            Vector2 axisInput = Vector2.zero;
                            rightDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

                            Primary2DAxisTouchedRight?.Invoke(axisInput);
                        }

                        if (buttonLeftPressed)
                        {
                            Vector2 axisInput = Vector2.zero;
                            leftDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axisInput);

                            Primary2DAxisTouchedLeft?.Invoke(axisInput);
                        }
                        break;

                    case InputHelpers.Button.Primary2DAxisClick:
                        Primary2DAxisClickedL = buttonLeftPressed;
                        Primary2DAxisClickedR = buttonRightPressed;

                        Primary2DAxisClickedLeft?.Invoke(Primary2DAxisClickedL);
                        Primary2DAxisClickedRight?.Invoke(Primary2DAxisClickedR);
                        break;

                    default:
                        break;
                }
            }
        }

    }
}

