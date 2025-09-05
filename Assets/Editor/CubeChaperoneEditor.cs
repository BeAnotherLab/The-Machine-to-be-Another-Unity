using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform))]
public class CubeChaperoneEditor : Editor
{
    private float delta = 0.1f; // Now editable!

    public override void OnInspectorGUI()
    {
        Transform t = (Transform)target;

        // Only allow editing if object has a BoxCollider and is roughly cube-shaped
        BoxCollider box = t.GetComponent<BoxCollider>();
        if (box == null)
        {
            EditorGUILayout.HelpBox("This object has no BoxCollider — this tool is intended for cube-like objects.", MessageType.Info);
            DrawDefaultInspector();
            return;
        }

        EditorGUILayout.LabelField("Chaperone Wall Adjustments", EditorStyles.boldLabel);

        // Add editable delta field
        delta = EditorGUILayout.FloatField("Increment", delta);
        delta = Mathf.Max(0.001f, delta); // Prevent negative or zero values

        GUILayout.BeginVertical("box");
        FaceAdjustButtons(t, Vector3.right, "Right (+X)");
        FaceAdjustButtons(t, -Vector3.right, "Left (-X)");
        FaceAdjustButtons(t, Vector3.up, "Top (+Y)");
        FaceAdjustButtons(t, -Vector3.up, "Bottom (-Y)");
        FaceAdjustButtons(t, Vector3.forward, "Front (+Z)");
        FaceAdjustButtons(t, -Vector3.forward, "Back (-Z)");
        GUILayout.EndVertical();

        EditorGUILayout.Space();
        DrawDefaultInspector();
    }

    private void FaceAdjustButtons(Transform t, Vector3 dir, string label)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(80));

        if (GUILayout.Button("-"))
        {
            AdjustFace(t, dir, -delta);
        }
        if (GUILayout.Button("+"))
        {
            AdjustFace(t, dir, delta);
        }

        GUILayout.EndHorizontal();
    }

    private void AdjustFace(Transform t, Vector3 faceDir, float moveAmount)
    {
        Vector3 scale = t.localScale;
        Vector3 pos = t.localPosition;

        // Determine axis: 0 = x, 1 = y, 2 = z
        int axis = faceDir.x != 0 ? 0 : faceDir.y != 0 ? 1 : 2;
        float dir = Mathf.Sign(faceDir[axis]);

        // Modify scale
        float oldScale = scale[axis];
        float newScale = Mathf.Max(0.01f, oldScale + (moveAmount * dir)); // avoid negative scale
        float deltaScale = newScale - oldScale;

        // Modify position
        float deltaPos = (deltaScale / 2f) * dir;

        scale[axis] = newScale;
        pos[axis] += deltaPos;

        Undo.RecordObject(t, "Adjust Cube Face");
        t.localScale = scale;
        t.localPosition = pos;
        EditorUtility.SetDirty(t);
    }
}
