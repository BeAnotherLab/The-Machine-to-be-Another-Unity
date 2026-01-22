using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScriptableObjectArchitecture;

public class DataLoader : MonoBehaviour
{
    public delegate void OnLoadLanguageButtonTexture(string language);
    public static OnLoadLanguageButtonTexture LoadLanguage; //TODO this is redundant with the language change game event
    
    [Header("Target ScriptableObject")]
    [SerializeField] private SequenceData sequenceData;
    [SerializeField] private List<string> _availableLanguages = new List<string>(); //the languages detected in the folder structure
    [SerializeField] private StringVariable _currentLanguage;
    [SerializeField] private StringGameEvent _languageChangeGameEvent;
    [SerializeField] private Translations _translations;
    [SerializeField] private StringGameEvent _setInstructionsTextFromKeyGameEvent;
    
    [System.Serializable]
    private class SequenceStepList
    {
        public List<SequenceStep> steps;
    }
    
    private void Start()
    {
        LoadSequenceFromJson(); //load the instructions sequence
        DiscoverLanguages();
        LoadSelectedLanguages(); //TODO make one?
    }
    
    public void LoadTranslations(string _languageCode) // called when language changed from button and on startup when discovering languages
    {
        string path = ContentPath.Translation(_languageCode);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Translation file not found at: {path}");
            _translations.Value = new Dictionary<string, string>();
            return;
        }
        try
        {
            string json = File.ReadAllText(path);
            _translations.Value = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading translations from {path}: {e.Message}");
            _translations.Value = new Dictionary<string, string>();
        }

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
         
            if (!_availableLanguages.Contains(lang)) _availableLanguages.Add(lang);
        }

        Debug.Log($"[DataLoader] Discovered languages: {string.Join(", ", _availableLanguages)}");
    }
    
    private void LoadSelectedLanguages()
    {
        string configPath = ContentPath.RootFolder("config.json");
        
        if (!File.Exists(configPath))
        {
            Debug.LogWarning($"Config file not found: {configPath}");
            return;
        }

        JArray selectedLanguagesArray = (JArray) JObject.Parse(File.ReadAllText(configPath))["selected_languages"];
        
        List<string> selectedLanguages = selectedLanguagesArray.ToObject<List<string>>();
        selectedLanguages.RemoveAll(language => !_availableLanguages.Contains(language)); // Keep only languages that actually exist

        Debug.Log($"[DataLoader] Selected languages: {string.Join(", ", selectedLanguages)}");
        
        foreach (string language in selectedLanguages) LoadLanguage(language); //let buttons know which languages to display
        
        LoadTranslations(selectedLanguages.First());
        _languageChangeGameEvent.Raise(selectedLanguages.First()); //set the language to the first one in the list
        _setInstructionsTextFromKeyGameEvent.Raise("idle");
    }
 
    
    
}