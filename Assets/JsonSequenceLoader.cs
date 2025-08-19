using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SequenceJsonLoader : MonoBehaviour
{
    [Header("Target ScriptableObject")]
    [SerializeField] private SequenceData sequenceData;

    [Header("Optional")]
    [SerializeField] private string jsonFileName = "sequence.json"; // Default name
    [SerializeField] private string sequenceFolder = "Content/Sequence"; // Relative to app root

    [Header("Debug")]
    [SerializeField] private bool logLoadedSteps = false;

    private void Awake()
    {
        LoadSequenceFromJson();
    }

    private void LoadSequenceFromJson()
    {
        string fullPath = Path.Combine(Application.dataPath, sequenceFolder, jsonFileName);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"Sequence JSON file not found at path: {fullPath}");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(fullPath);
            SequenceStepList stepList = JsonUtility.FromJson<SequenceStepList>(jsonContent);

            if (stepList?.steps == null)
            {
                Debug.LogError("Failed to deserialize sequence steps.");
                return;
            }

            sequenceData.steps = stepList.steps;

            if (logLoadedSteps)
            {
                Debug.Log($"Loaded {stepList.steps.Count} steps from sequence.json");
                foreach (var step in stepList.steps)
                {
                    Debug.Log($"Step at {step.time}s → Text: {step.textKey}, Audio: {step.audio}, Visual: {step.visual}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading sequence file: {e.Message}");
        }
    }

    [System.Serializable]
    private class SequenceStepList
    {
        public List<SequenceStep> steps;
    }
}