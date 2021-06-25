using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine;

namespace UVR
{
    [CustomEditor(typeof(UV_AdvancedGrabInteractable), true)]
    [CanEditMultipleObjects]
    public class UV_AdvancedGrabInteractableEditor : XRGrabInteractableEditor
    {
        SerializedProperty advancedGrab;

        SerializedProperty touchableBy;
        SerializedProperty grabbableBy;
        SerializedProperty activableBy;
        ////SerializedProperty laserInteractable;
        SerializedProperty activeOnTouch;
        SerializedProperty holdButtonToGrab;
        SerializedProperty holdButtonToActivate;


        protected override void OnEnable()
        {
            base.OnEnable();

            advancedGrab = serializedObject.FindProperty("m_AdvancedGrab");
            touchableBy = serializedObject.FindProperty("m_TouchableBy");
            grabbableBy = serializedObject.FindProperty("m_GrabbableBy");
            activableBy = serializedObject.FindProperty("m_ActivableBy");
            ////laserInteractable = serializedObject.FindProperty("m_LaserInteractable");
            activeOnTouch = serializedObject.FindProperty("m_ActiveOnTouch");
            holdButtonToGrab = serializedObject.FindProperty("m_HoldButtonToGrab");
            holdButtonToActivate = serializedObject.FindProperty("m_HoldButtonToActivate");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(advancedGrab);

            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(3));

            EditorGUILayout.LabelField("Custom Variables", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(touchableBy);

            if (touchableBy.enumValueIndex == (int)GlobalVariables.XRNodeEvo.Everything)
            {
                EditorGUILayout.PropertyField(grabbableBy);
                EditorGUILayout.PropertyField(activableBy);
            }
            else
            {
                grabbableBy.enumValueIndex = touchableBy.enumValueIndex;
                activableBy.enumValueIndex = touchableBy.enumValueIndex;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(activeOnTouch);
            EditorGUILayout.PropertyField(holdButtonToGrab);
            EditorGUILayout.PropertyField(holdButtonToActivate);
            ////EditorGUILayout.PropertyField(laserInteractable);

            serializedObject.ApplyModifiedProperties();
        }
    }
}



