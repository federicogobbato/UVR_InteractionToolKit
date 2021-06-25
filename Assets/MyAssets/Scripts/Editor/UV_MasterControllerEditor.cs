using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UV_MasterController))]
[ExecuteInEditMode]
public class UV_MasterControllerEditor : Editor
{
    SerializedProperty MainCamera;
    SerializedProperty UIInputModule;
    SerializedProperty StartingPosition;
    SerializedProperty RightTeleportInteractor;
    SerializedProperty LeftTeleportInteractor;
    SerializedProperty InputTeleportType;
    SerializedProperty TeleportRightActive;
    SerializedProperty TeleportLeftActive;
    SerializedProperty OnTeleport;
    SerializedProperty TeleportSensibility;
    SerializedProperty RightLaserInteractor;
    SerializedProperty LeftLaserInteractor;
    SerializedProperty AlwaysActive;
    SerializedProperty RightLaserActive;
    SerializedProperty LeftLaserActive;
    SerializedProperty RightInteractorUI;
    SerializedProperty LeftInteractorUI;
    SerializedProperty InteractorUIActive;
    SerializedProperty RightDirectInteractor;
    SerializedProperty LeftDirectInteractor;

    protected virtual void OnEnable()
    {
        MainCamera = serializedObject.FindProperty("MainCamera");
        UIInputModule = serializedObject.FindProperty("UIInputModule");
        StartingPosition = serializedObject.FindProperty("StartingPosition");
        RightTeleportInteractor = serializedObject.FindProperty("RightTeleportInteractor");
        LeftTeleportInteractor = serializedObject.FindProperty("LeftTeleportInteractor");
        InputTeleportType = serializedObject.FindProperty("m_InputTeleportType");
        TeleportRightActive = serializedObject.FindProperty("TeleportRightActive");
        TeleportLeftActive = serializedObject.FindProperty("TeleportLeftActive");
        OnTeleport = serializedObject.FindProperty("OnTeleportFade");
        TeleportSensibility = serializedObject.FindProperty("TeleportSensibility");
        RightLaserInteractor = serializedObject.FindProperty("RightLaserInteractor");
        LeftLaserInteractor = serializedObject.FindProperty("LeftLaserInteractor");
        AlwaysActive = serializedObject.FindProperty("AlwaysActive");
        RightLaserActive = serializedObject.FindProperty("RightLaserActive");
        LeftLaserActive = serializedObject.FindProperty("LeftLaserActive");
        RightInteractorUI = serializedObject.FindProperty("RightInteractorUI");
        LeftInteractorUI = serializedObject.FindProperty("LeftInteractorUI");
        InteractorUIActive = serializedObject.FindProperty("InteractorUIActive");
        RightDirectInteractor = serializedObject.FindProperty("RightDirectInteractor");
        LeftDirectInteractor = serializedObject.FindProperty("LeftDirectInteractor");
    }

    public override void OnInspectorGUI()
    {
        GUIStyle LabelStyle = new GUIStyle();
        LabelStyle.fontStyle = FontStyle.Bold;
        LabelStyle.fontSize = 13;

        GUIStyle Area = new GUIStyle(GUI.skin.box);
        Area.padding = new RectOffset(15, 20, 10, 10);

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((UV_MasterController)target), typeof(UV_MasterController), false);
        GUI.enabled = true;

        serializedObject.Update();

        GUILayout.Label("Setup", LabelStyle);
        GUILayout.BeginVertical(Area);
        EditorGUILayout.PropertyField(MainCamera);
        EditorGUILayout.PropertyField(StartingPosition);
        GUILayout.EndVertical();
        
        EditorGUILayout.Space();

        GUILayout.Label("Teleport", LabelStyle);
        GUILayout.BeginVertical(Area);
        EditorGUILayout.PropertyField(RightTeleportInteractor);
        EditorGUILayout.PropertyField(LeftTeleportInteractor);
        EditorGUILayout.PropertyField(InputTeleportType);
        EditorGUILayout.PropertyField(TeleportRightActive);
        EditorGUILayout.PropertyField(TeleportLeftActive);
        EditorGUILayout.PropertyField(TeleportSensibility);
        EditorGUILayout.PropertyField(OnTeleport);
        GUILayout.EndVertical();

        EditorGUILayout.Space();
        
        GUILayout.Label("Laser", LabelStyle);
        GUILayout.BeginVertical(Area);
        EditorGUILayout.PropertyField(RightLaserInteractor);
        EditorGUILayout.PropertyField(LeftLaserInteractor);
        EditorGUILayout.PropertyField(AlwaysActive);
        EditorGUILayout.PropertyField(RightLaserActive);
        EditorGUILayout.PropertyField(LeftLaserActive);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.Label("UI_Laser", LabelStyle);
        GUILayout.BeginVertical(Area);
        EditorGUILayout.PropertyField(UIInputModule);
        EditorGUILayout.PropertyField(RightInteractorUI);
        EditorGUILayout.PropertyField(LeftInteractorUI);
        EditorGUILayout.PropertyField(InteractorUIActive);
        GUILayout.EndVertical();

        EditorGUILayout.Space();

        GUILayout.Label("Interactors", LabelStyle);
        GUILayout.BeginVertical(Area);
        EditorGUILayout.PropertyField(RightDirectInteractor);
        EditorGUILayout.PropertyField(LeftDirectInteractor);
        GUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        //var t = (target as MasterController);

    }
}
