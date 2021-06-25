using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEditor.XR.Interaction.Toolkit
{
    [CustomEditor(typeof(XRSimpleInteractable), true)]
    [CanEditMultipleObjects]
    public class XRSimpleInteractableEditor : Editor
    {
        SerializedProperty m_OnFirstHoverEnter;
        SerializedProperty m_OnHoverEnter;
        SerializedProperty m_OnHoverExit;
        SerializedProperty m_OnLastHoverExit;
        SerializedProperty m_OnSelectEnter;
        SerializedProperty m_OnSelectExit;
        SerializedProperty m_OnActivate;
        SerializedProperty m_OnDeactivate;
        SerializedProperty m_Colliders;
        SerializedProperty m_InteractionLayerMask;

        bool m_showInteractableEvents = false;


        static class Tooltips
        {
            public static readonly GUIContent colliders = new GUIContent("Colliders", "Colliders to include when selecting/interacting with an interactable");
            public static readonly GUIContent interactionLayerMask = new GUIContent("InteractionLayerMask", "Only Interactors with this LayerMask will interact with this Interactable.");
        }

        protected virtual void OnEnable()
        {
            m_OnFirstHoverEnter = serializedObject.FindProperty("m_OnFirstHoverEnter");
            m_OnHoverEnter = serializedObject.FindProperty("m_OnHoverEnter");
            m_OnHoverExit = serializedObject.FindProperty("m_OnHoverExit");
            m_OnLastHoverExit = serializedObject.FindProperty("m_OnLastHoverExit");
            m_OnSelectEnter = serializedObject.FindProperty("m_OnSelectEnter");
            m_OnSelectExit = serializedObject.FindProperty("m_OnSelectExit");
            m_OnActivate = serializedObject.FindProperty("m_OnActivate");
            m_OnDeactivate = serializedObject.FindProperty("m_OnDeactivate");
            m_Colliders = serializedObject.FindProperty("m_Colliders");
            m_InteractionLayerMask = serializedObject.FindProperty("m_InteractionLayerMask");
        }

        public override void OnInspectorGUI()
        {

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((XRSimpleInteractable)target), typeof(XRSimpleInteractable), false);
            GUI.enabled = true;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Colliders, Tooltips.colliders, true);

            EditorGUILayout.PropertyField(m_InteractionLayerMask, Tooltips.interactionLayerMask);

            m_showInteractableEvents = EditorGUILayout.Foldout(m_showInteractableEvents, "Interactable Events");

            if (m_showInteractableEvents)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(m_OnFirstHoverEnter);
                EditorGUILayout.PropertyField(m_OnHoverEnter);
                EditorGUILayout.PropertyField(m_OnHoverExit);
                EditorGUILayout.PropertyField(m_OnLastHoverExit);
                EditorGUILayout.PropertyField(m_OnSelectEnter);
                EditorGUILayout.PropertyField(m_OnSelectExit);
                EditorGUILayout.PropertyField(m_OnActivate);
                EditorGUILayout.PropertyField(m_OnDeactivate);
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}
