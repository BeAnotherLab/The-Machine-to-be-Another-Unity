using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class JsonSequenceLoader : MonoBehaviour
{
    [Header("Target ScriptableObject")]
    [SerializeField] private SequenceData sequenceData;

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
            SequenceStepList stepList = JsonConvert.DeserializeObject<SequenceStepList>(jsonContent);

            if (stepList?.steps == null || stepList.steps.Count == 0)
            {
                Debug.LogError("[JsonSequenceLoader] Failed to parse or empty sequence.");
                return;
            }

            sequenceData.steps = stepList.steps;
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