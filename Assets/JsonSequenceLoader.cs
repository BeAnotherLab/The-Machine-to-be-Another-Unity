using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonSequenceLoader : MonoBehaviour
{
    [Header("Target ScriptableObject")]
    [SerializeField] private SequenceData sequenceData;

    [Header("Debug")]
    [SerializeField] private bool logLoadedSteps = false;

    private void Awake()
    {
        LoadSequenceFromJson();
    }

    private void LoadSequenceFromJson()
    {
        string fullPath = ContentPath.Sequence("sequence.json");

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"[JsonSequenceLoader] Sequence JSON file not found at path: {fullPath}");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(fullPath);
            SequenceStepList stepList = JsonUtility.FromJson<SequenceStepList>(jsonContent);

            if (stepList?.steps == null || stepList.steps.Count == 0)
            {
                Debug.LogError("[JsonSequenceLoader] Failed to parse or empty sequence.");
                return;
            }

            sequenceData.steps = stepList.steps;

            if (logLoadedSteps)
            {
                Debug.Log($"[JsonSequenceLoader] Loaded {stepList.steps.Count} steps from sequence.json");
                foreach (var step in stepList.steps)
                {
                    Debug.Log($"→ {step.time}s: {step.textKey} | {step.audio} | {step.visual} | Actions: {string.Join(", ", step.actions)}");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[JsonSequenceLoader] Exception while reading sequence: {ex.Message}");
        }
    }

    [System.Serializable]
    private class SequenceStepList
    {
        public List<SequenceStep> steps;
    }
}