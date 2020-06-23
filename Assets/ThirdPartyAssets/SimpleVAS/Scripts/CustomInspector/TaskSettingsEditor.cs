using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UnityPsychBasics {

    [CustomEditor(typeof(TaskSettings))]
    public class TaskSettingsEditor : Editor {

        protected int previousSize;
        protected static bool scaleLegendsFoldout;
        
        override public void OnInspectorGUI() {

           var myScript = target as TaskSettings;

            myScript.sceneBeforeLastCondition = EditorGUILayout.TextField("Scene Before Last", myScript.sceneBeforeLastCondition);
            myScript.sceneAfterLastCondition = EditorGUILayout.TextField("Scene Before Last", myScript.sceneAfterLastCondition);
            EditorGUILayout.Space();

            scaleLegendsFoldout = EditorGUILayout.Foldout(scaleLegendsFoldout,"Set legends for scales"); //GUILayout.Label("Set legends for scales");
            if(scaleLegendsFoldout){

                EditorGUI.indentLevel++;
                myScript.minVASLabel = EditorGUILayout.TextField("VAS left label", myScript.minVASLabel);
                myScript.midVASLabel = EditorGUILayout.TextField("VAS middle label", myScript.midVASLabel);
                myScript.maxVASLabel = EditorGUILayout.TextField("VAS right label", myScript.maxVASLabel);

                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("likertItems"), new GUIContent("Likert Items"), true);
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;          
            }

            EditorGUILayout.Space();

            serializedObject.Update();
            serializedObject.FindProperty("withinScene").boolValue = EditorGUILayout.Toggle("Tasks within Scene", myScript.withinScene);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(myScript.withinScene))) {
                if (group.visible == true)
                    TasksWithinSceneInspectorOptions(myScript);
                else
                    TasksInDifferentScenesOptions(myScript);
            }
        }

        private void TasksWithinSceneInspectorOptions(TaskSettings myScript) {
            EditorGUI.indentLevel++;
            myScript.numberOfConditions = EditorGUILayout.IntField("Number of Conditions", myScript.numberOfConditions);
            EditorGUILayout.Space();

            SetLists(myScript);

            EditorGUI.indentLevel--;
        }

        private void TasksInDifferentScenesOptions(TaskSettings myScript){
            EditorGUI.indentLevel++;
            serializedObject.Update();
            serializedObject.FindProperty("shuffleBool").boolValue = EditorGUILayout.Toggle("Shuffle", myScript.shuffleBool);
            serializedObject.FindProperty("useImageBool").boolValue = EditorGUILayout.Toggle("Use Image", myScript.useImageBool);
            serializedObject.FindProperty("useAnalogueScaleBool").boolValue = EditorGUILayout.Toggle("Use VAS", myScript.useAnalogueScaleBool);
            serializedObject.FindProperty("useMouseBool").boolValue = EditorGUILayout.Toggle("Use Mouse Selector", myScript.useMouseBool);
            //myScript.shuffleBool = EditorGUILayout.Toggle("Shuffle", myScript.shuffleBool);
            //myScript.useImageBool = EditorGUILayout.Toggle("Use Image", myScript.useImageBool);
            //myScript.useAnalogueScaleBool = EditorGUILayout.Toggle("Use Image", myScript.useAnalogueScaleBool);
            //myScript.useMouseBool = EditorGUILayout.Toggle("Use Mouse Selector", myScript.useMouseBool);
            serializedObject.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }

         private void SetLists(TaskSettings myScript) {

            serializedObject.Update();
            serializedObject.FindProperty("shuffle").arraySize = myScript.numberOfConditions;
            serializedObject.FindProperty("useImage").arraySize = myScript.numberOfConditions;
            serializedObject.FindProperty("analogueScale").arraySize = myScript.numberOfConditions;
            serializedObject.FindProperty("useMouseClickSelector").arraySize = myScript.numberOfConditions;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("shuffle"), new GUIContent("Shuffle"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useImage"), new GUIContent("Use Image"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("analogueScale"), new GUIContent("Use VAS"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useMouseClickSelector"), new GUIContent("Use Mouse Selector"), true);
            serializedObject.ApplyModifiedProperties();

        }

        }
}