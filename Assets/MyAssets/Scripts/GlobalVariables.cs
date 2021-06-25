using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UVR
{
    public class GlobalVariables : MonoBehaviour
    {
        [Serializable]
        public class UVR_BaseEvent : UnityEvent { }

        // Summary:
        // Enumeration of XR nodes which can be updated by XR input or sent haptic data.
        public enum XRNodeEvo
        {
            // Summary:
            //     Node representing the left eye.
            LeftEye = 0,
            //
            // Summary:
            //     Node representing the right eye.
            RightEye = 1,
            //
            // Summary:
            //     Node representing a point between the left and right eyes.
            CenterEye = 2,
            //
            // Summary:
            //     Node representing the user's head.
            Head = 3,
            //
            // Summary:
            //     Node representing the left hand.
            LeftHand = 4,
            //
            // Summary:
            //     Node representing the right hand.
            RightHand = 5,
            //
            // Summary:
            //     Represents a tracked game Controller not associated with a specific hand.
            GameController = 6,
            //
            // Summary:
            //     Represents a stationary physical device that can be used as a point of reference
            //     in the tracked area.
            TrackingReference = 7,
            //
            // Summary:
            //     Represents a physical device that provides tracking data for objects to which
            //     it is attached.
            HardwareTracker = 8,

            Everything = 9,
        }
    }

    [System.Serializable]
    public class Vector3State
    {
        public bool xState;
        public bool yState;
        public bool zState;

        public static Vector3State False
        {
            get
            {
                return new Vector3State(false, false, false);
            }
        }

        public static Vector3State True
        {
            get
            {
                return new Vector3State(true, true, true);
            }
        }

        public Vector3State(bool x, bool y, bool z)
        {
            xState = x;
            yState = y;
            zState = z;
        }
    }
}

