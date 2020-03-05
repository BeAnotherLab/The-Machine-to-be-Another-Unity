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
        
        if (GUILayout.Button("Other is ready")) statusManager.OtherUserIsReady();
        if (GUILayout.Button("Other is gone")) statusManager.OtherLeft();
        if (GUILayout.Button("Self is ready")) statusManager.ThisUserIsReady();
        if (GUILayout.Button("Self is Gone")) statusManager.StopExperience();
    }
}
