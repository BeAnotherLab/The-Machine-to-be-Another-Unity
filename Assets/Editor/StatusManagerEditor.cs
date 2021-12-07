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

        var userStates = statusManager.userStatesVariable.Value;
        var userStatesEvent = statusManager.userStatesGameEvent;
        
        if (GUILayout.Button("Other is ready"))
        {
            userStates.otherStatus = UserStatus.readyToStart; //statusManager.OtherUserIsReady();
            userStatesEvent.Raise(userStates);
        }

        if (GUILayout.Button("Other is gone"))
        {
            statusManager.userStatesVariable.Value.otherStatus = UserStatus.headsetOff; //statusManager.OtherLeft();
            userStatesEvent.Raise(userStates);
        }
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Other put headset on"))
        {
            statusManager.userStatesVariable.Value.otherStatus = UserStatus.headsetOn; //statusManager.OtherPutHeadsetOn();
            userStatesEvent.Raise(userStates);
        }

        if (GUILayout.Button("Self put headset on"))
        {
            statusManager.userStatesVariable.Value.selfStatus = UserStatus.headsetOn; //statusManager.SelfPutHeadsetOn();
            userStatesEvent.Raise(userStates);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Self is ready"))
        {
            statusManager.userStatesVariable.Value.otherStatus = UserStatus.readyToStart; //statusManager.ThisUserIsReady();
            userStatesEvent.Raise(userStates);
        }

        if (GUILayout.Button("Self is gone"))
        {
            statusManager.userStatesVariable.Value.otherStatus = UserStatus.headsetOff; //statusManager.SelfRemovedHeadset();
            userStatesEvent.Raise(userStates);
        } 
        
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        
        if (GUILayout.Button("Serial ready"))
            statusManager.SerialReady();
    }
}
