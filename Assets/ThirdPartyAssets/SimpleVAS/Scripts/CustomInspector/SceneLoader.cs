using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityPsychBasics
{/*
    [CustomEditor(typeof(LoadScene))]
    public class SceneLoader : Editor {


        override public void OnInspectorGUI()
        {
            var myScript = target as LoadScene;

            myScript.sceneToLoad = EditorGUILayout.TextField("Scene To Load", myScript.sceneToLoad);
            myScript.changeOnKey = EditorGUILayout.Toggle("Change On Key", myScript.changeOnKey);
            myScript.changeAtTime = EditorGUILayout.Toggle("Change At Time", myScript.changeAtTime);            

            //scaleLegendsFoldout = EditorGUILayout.Foldout(scaleLegendsFoldout, "Set legends for scales"); //GUILayout.Label("Set legends for scales");
            if (myScript.changeAtTime){
                EditorGUI.indentLevel++;
                SerializedProperty _sceneDuration = serializedObject.FindProperty("sceneDuration");
                EditorGUILayout.PropertyField(_sceneDuration);
                //myScript.sceneDuration = EditorGUILayout.FloatField("Scene Duration", myScript.sceneDuration);;
            }


            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }*/
 }
