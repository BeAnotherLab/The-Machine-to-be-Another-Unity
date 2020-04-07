using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(StatusManager))]
public class StatusManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        StatusManager statusManager = (StatusManager) target;

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Other is ready")) statusManager.OtherUserIsReady();
        if (GUILayout.Button("Other is gone")) statusManager.OtherLeft();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Other put headset on")) statusManager.OtherPutHeadsetOn();
        if (GUILayout.Button("Self put headset on")) statusManager.SelfPutHeadsetOn();
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Self is ready")) statusManager.ThisUserIsReady();
        if (GUILayout.Button("Self is gone")) statusManager.SelfRemovedHeadset();
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Serial ready")) statusManager.SerialReady();


    }
}
