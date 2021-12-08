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

        var selfState = statusManager.selfState.Value;
        var otherState = statusManager.otherState.Value;
        var selfStateEvent = statusManager.selfStateGameEvent;
        var otherStateEvent = statusManager.otherStateGameEvent;
        
        if (GUILayout.Button("Other is ready"))
        {
            otherState = UserState.readyToStart; //statusManager.OtherUserIsReady();
            otherStateEvent.Raise(otherState);
        }

        if (GUILayout.Button("Other is gone"))
        {
            otherState = UserState.headsetOff; //statusManager.OtherLeft();
            otherStateEvent.Raise(otherState);
        }
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Other put headset on"))
        {
            otherState = UserState.headsetOn; //statusManager.OtherPutHeadsetOn();
            otherStateEvent.Raise(otherState);
        }

        if (GUILayout.Button("Self put headset on"))
        {
            selfState = UserState.headsetOn; //statusManager.SelfPutHeadsetOn();
            selfStateEvent.Raise(selfState);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Self is ready"))
        {
            selfState = UserState.readyToStart; //statusManager.ThisUserIsReady();
            selfStateEvent.Raise(selfState);
        }

        if (GUILayout.Button("Self is gone"))
        {
            selfState = UserState.headsetOff; //statusManager.SelfRemovedHeadset();
            selfStateEvent.Raise(selfState);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Serial ready"))
            statusManager.SerialReady();
    }
}
