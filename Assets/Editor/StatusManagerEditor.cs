using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
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

        var selfState = statusManager.selfState;
        var otherState = statusManager.otherState;
        var selfStateEvent = statusManager.selfStateGameEvent;
        var otherStateEvent = statusManager.otherStateGameEvent;
        
        if (GUILayout.Button("Other is ready"))
        {
            otherState.Value = UserState.readyToStart; //statusManager.OtherUserIsReady();
            otherStateEvent.Raise(UserState.readyToStart);
        }

        if (GUILayout.Button("Other is gone"))
        {
            otherState.Value = UserState.headsetOff; //statusManager.OtherLeft();
            otherStateEvent.Raise(UserState.headsetOff);
        }
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Other put headset on"))
        {
            otherState.Value = UserState.headsetOn; 
            otherStateEvent.Raise(UserState.headsetOn);
        }

        if (GUILayout.Button("Self put headset on"))
        {
            selfState.Value = UserState.headsetOn; //statusManager.SelfPutHeadsetOn();
            selfStateEvent.Raise(UserState.headsetOn);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Self is ready"))
        {
            selfState.Value = UserState.readyToStart; //statusManager.ThisUserIsReady();
            selfStateEvent.Raise(UserState.readyToStart);
        }

        if (GUILayout.Button("Self is gone"))
        {
            selfState.Value = UserState.headsetOff; //statusManager.SelfRemovedHeadset();
            selfStateEvent.Raise(UserState.headsetOff);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Serial ready"))
            statusManager.SerialReady();
    }
}
