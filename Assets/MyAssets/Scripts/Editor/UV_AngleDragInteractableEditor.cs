﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.XR.Interaction.Toolkit;
using UnityEngine;

namespace UVR
{
    [CustomEditor(typeof(UV_AngleDragInteractable), true)]
    [CanEditMultipleObjects]
    public class UV_AngleDragInteractableEditor : XRSimpleInteractableEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space(10);
            GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(3));
            EditorGUILayout.LabelField("Custom Variables", EditorStyles.boldLabel);

            var t = (target as UV_AngleDragInteractable);

            FieldInfo[] childFields = target.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in childFields)
            {
                if (field.IsPublic || field.GetCustomAttribute(typeof(SerializeField)) != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}


