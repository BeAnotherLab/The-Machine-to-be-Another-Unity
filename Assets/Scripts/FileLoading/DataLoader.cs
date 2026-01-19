using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DataLoader : MonoBehaviour
{
    public delegate void OnLoadLanguageButtonTexture(string language);
    public static OnLoadLanguageButtonTexture LoadLanguageButtonTexture;
    
    [Header("Target ScriptableObject")]
    [SerializeField] private SequenceData sequenceData;

    private List<string> _availableLanguages = new List<string>(); //TODO use fixed length array?
    private List<string> _selectedLanguages = new List<string>();
    
    
    [System.Serializable]
    private class SequenceStepList
    {
        public List<SequenceStep> steps;
    }
    
    private void Start()
    {
        LoadSequenceFromJson();
        DiscoverLanguages();
        LoadSelectedLanguages();
        //TODO discover languages available
        //TODO load languages selected in config.json
        //TODO notify flags loaders
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

    
    private void DiscoverLanguages()
    {
        string flagsPath = ContentPath.Static("");
        if (!Directory.Exists(flagsPath))
        {
            Debug.LogWarning($"[DataLoader] Static folder not found: {flagsPath}");
            return;
        }

        foreach (string file in Directory.GetFiles(flagsPath, "flag_*.png"))
        {
            string lang = Path.GetFileNameWithoutExtension(file).Replace("flag_", "");
            if (!_availableLanguages.Contains(lang))
                _availableLanguages.Add(lang);
        }

        Debug.Log($"[DataLoader] Discovered languages: {string.Join(", ", _availableLanguages)}");
    }
    
    
    private void LoadSelectedLanguages()
    {
        string configPath = ContentPath.RootFolder("config.json");
        _selectedLanguages = GetVisibleLanguages(configPath);

        // Keep only languages that actually exist
        _selectedLanguages.RemoveAll(lang => !_availableLanguages.Contains(lang));

        Debug.Log($"[DataLoader] Selected languages: {string.Join(", ", _selectedLanguages)}");
        
        foreach (string language in _selectedLanguages)
            LoadLanguageButtonTexture(language);
    }
    
    public List<string> GetVisibleLanguages(string configPath)
    {
        if (!File.Exists(configPath))
        {
            Debug.LogWarning($"Config file not found: {configPath}");
            return new List<string>();
        }

        string json = File.ReadAllText(configPath);
        JObject obj = JObject.Parse(json);
        JArray langs = (JArray)obj["visible_languages"];
        List<string> result = langs.ToObject<List<string>>();
        return result;
    }
    
}