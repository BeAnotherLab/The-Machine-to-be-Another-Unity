using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Uduino
{
    [CustomEditor(typeof(UduinoInterface_Serial))]
    public class UduinoInterfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Interface", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }
    }
}