#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeightPresenceDetection))]
public class HeightPresenceDetectionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        HeightPresenceDetection detector = (HeightPresenceDetection)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Runtime Debug Info", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            // -------------------------------
            // Display current headset Y-position as a read-only slider
            // -------------------------------
            float min = detector.removalYThreshold - 0.5f;
            float max = detector.wornYThreshold + 0.5f;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Slider("Current Y", detector.currentHeadY, min, max);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(5);

            // -------------------------------
            // Display current state as a color-coded label
            // -------------------------------
            UserState state = detector.selfState.Value;

            string label = state == UserState.headsetOn ? "🟢 Headset On" : "🔴 Headset Off";
            Color textColor = state == UserState.headsetOn ? Color.green : Color.red;

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = textColor;
            labelStyle.fontSize = 12;

            EditorGUILayout.LabelField(label, labelStyle);
        }
        else
        {
            EditorGUILayout.HelpBox("Runtime info (slider and state) will appear during Play Mode.", MessageType.Info);
        }
    }
}
#endif