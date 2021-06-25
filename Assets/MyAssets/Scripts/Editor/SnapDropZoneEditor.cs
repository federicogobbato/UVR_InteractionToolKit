using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace UVR
{
    [CustomEditor(typeof(SnapDropZone))]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class SnapDropZoneEditor : Editor
    {
        SerializedProperty showInEditor;
        SerializedProperty showSphereCollider;
        SerializedProperty objectToDropOrClone;
        SerializedProperty sphereColliderMaterial;
        SerializedProperty snapDropObjectMaterial;
        SerializedProperty validDropMaterial;
        SerializedProperty type;
        SerializedProperty dropOnThrow;
        SerializedProperty grabbableAfterDrop;
        SerializedProperty delayBeforeDisappear;
        SerializedProperty connection;
        SerializedProperty rigidbodyToConnect;
        SerializedProperty showMeshMaterial;
        SerializedProperty showMeshMaterialBeforeDrop;
        SerializedProperty onEnterEvent;
        SerializedProperty onExitEvent;
        SerializedProperty onDropEvent;

        bool m_ShowInteractorEvents;

        protected virtual void OnEnable()
        {
            showInEditor = serializedObject.FindProperty("m_ShowInEditor");
            showSphereCollider = serializedObject.FindProperty("m_ShowSphereCollider");
            objectToDropOrClone = serializedObject.FindProperty("m_SampleObject");
            sphereColliderMaterial = serializedObject.FindProperty("m_SphereColliderMaterial");
            snapDropObjectMaterial = serializedObject.FindProperty("m_BaseDropMaterial");
            validDropMaterial = serializedObject.FindProperty("m_ValidDropMaterial");
            type = serializedObject.FindProperty("m_Type");
            dropOnThrow = serializedObject.FindProperty("m_DroppableOnThrow");
            grabbableAfterDrop = serializedObject.FindProperty("m_GrabbableAfterDrop");
            delayBeforeDisappear = serializedObject.FindProperty("m_DelayBeforeDisappear");
            connection = serializedObject.FindProperty("m_Connection");
            rigidbodyToConnect = serializedObject.FindProperty("m_RigidbodyToConnect");
            showMeshMaterial = serializedObject.FindProperty("m_ShowBaseMeshMaterial");
            showMeshMaterialBeforeDrop = serializedObject.FindProperty("m_ShowValidMeshMaterial");
            onEnterEvent = serializedObject.FindProperty("OnEnter");
            onExitEvent = serializedObject.FindProperty("OnExit");
            onDropEvent = serializedObject.FindProperty("OnDrop");

            var t = (target as SnapDropZone);

            t.LookForSampleObject();
            t.LookForSphereCollider();
        }

        public override void OnInspectorGUI()
        {
            GUIStyle LabelStyle = new GUIStyle();
            LabelStyle.fontStyle = FontStyle.BoldAndItalic;
            LabelStyle.fontSize = 12;

            GUIStyle MainArea = new GUIStyle(GUI.skin.box);
            MainArea.padding = new RectOffset(15, 30, 10, 10);

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((SnapDropZone)target), typeof(SnapDropZone), false);
            GUI.enabled = true;

            //Used later to check if the objectPrefab is changed
            Object prevObjectToDropOrClone = null;
            if (objectToDropOrClone != null)
            {
                prevObjectToDropOrClone = objectToDropOrClone.objectReferenceValue;
            }
            SnapDropZone.Type prevType = (SnapDropZone.Type)type.enumValueIndex;

            serializedObject.Update();

            EditorGUILayout.Space();

            GUILayout.Label("Editor", LabelStyle);
            if(type.enumValueIndex != (int)SnapDropZone.Type.DROP_EVERYTHING) EditorGUILayout.PropertyField(showInEditor);
            EditorGUILayout.PropertyField(showSphereCollider);
            EditorGUILayout.PropertyField(sphereColliderMaterial);
            
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(3));
            EditorGUILayout.Space();

            if(type.enumValueIndex != (int)SnapDropZone.Type.DROP_EVERYTHING)
            {
                EditorGUILayout.PropertyField(objectToDropOrClone);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(type);

            if (type.enumValueIndex != (int)SnapDropZone.Type.CLONE && type.enumValueIndex != (int)SnapDropZone.Type.DROP_EVERYTHING)
                GUILayout.BeginVertical(MainArea);

            if (type.enumValueIndex == (int)SnapDropZone.Type.DROP_AND_DISAPPEAR)
            {
                EditorGUILayout.PropertyField(delayBeforeDisappear);
            }
            else if (type.enumValueIndex == (int)SnapDropZone.Type.DROP)
            {
                EditorGUILayout.PropertyField(connection);
                if (connection.enumValueIndex == (int)SnapDropZone.Connection.JOINT)
                {
                    EditorGUILayout.PropertyField(rigidbodyToConnect);
                }

                EditorGUILayout.PropertyField(grabbableAfterDrop);
            }

            if (type.enumValueIndex != (int)SnapDropZone.Type.CLONE && type.enumValueIndex != (int)SnapDropZone.Type.DROP_EVERYTHING)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(dropOnThrow);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(showMeshMaterial);
                EditorGUILayout.PropertyField(showMeshMaterialBeforeDrop);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(snapDropObjectMaterial);
                EditorGUILayout.PropertyField(validDropMaterial);
                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            m_ShowInteractorEvents = EditorGUILayout.Toggle("Show Events", m_ShowInteractorEvents);
            if (m_ShowInteractorEvents)
            {
                // UnityEvents have not yet supported Tooltips
                EditorGUILayout.PropertyField(onEnterEvent);
                EditorGUILayout.PropertyField(onExitEvent);
                EditorGUILayout.PropertyField(onDropEvent);
            }

            serializedObject.ApplyModifiedProperties();

            var t = (target as SnapDropZone);

            if (EditorApplication.isPlaying) return;

            if (showInEditor.boolValue && 
                objectToDropOrClone.objectReferenceValue != null)
            {
                if (t.CheckThisIsPrefab())
                {
                    //Destroy the object to drop or clone if is changed of its type is changed 
                    if ((prevObjectToDropOrClone != null &&
                        prevObjectToDropOrClone != objectToDropOrClone.objectReferenceValue) ||
                        prevType != (SnapDropZone.Type)type.enumValueIndex)
                    {
                        t.DestroySample();
                    }

                    t.GenerateSampleEditorObject();

                    t.FollowSDZ();
                }
            }
            else
            {
                t.DestroySample();
            }

            if (showSphereCollider.boolValue)
            {
                t.GenerateSphereCollider();
            }
            else
            {
                t.DestroySphereCollider();
            }
        }
    }
}

