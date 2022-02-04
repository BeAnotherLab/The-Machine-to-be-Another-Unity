using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using Debug = DebugFile;

public class LocalizationManager : MonoBehaviour {

    public static LocalizationManager instance;

    [SerializeField] private string[] localizationTexts; //TODO autoload

    [SerializeField] private TrackAsset _germanTrack;
    [SerializeField] private TrackAsset _englishTrack;
    
    private Dictionary<string, string> localizedText;
    private string missingTextString = "Localized text not found";

    // Use this for initialization
    void Awake () 
    {
        if (instance == null) {
            instance = this;
        } else if (instance != this)
        {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (gameObject);
    }

    private void Start()
    {
        TimelineAsset timelineAsset = (TimelineAsset) StatusManager.instance.instructionsTimeline.playableAsset;
        _englishTrack = timelineAsset.GetOutputTrack(0);
        _germanTrack = timelineAsset.GetOutputTrack(1);
        LoadLocalizedText(localizationTexts[4]);
    }

    public void LoadLocalizedText(int id)
    {
        LoadLocalizedText(localizationTexts[id]);
    }
    
    public void LoadLocalizedText(string fileName, bool resend = false)
    {
        localizedText = new Dictionary<string, string> ();
        string filePath = Path.Combine (Application.streamingAssetsPath, fileName);

        if (File.Exists (filePath)) {
            string dataAsJson = File.ReadAllText (filePath);
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData> (dataAsJson);

            for (int i = 0; i < loadedData.items.Length; i++) 
            {
                localizedText.Add (loadedData.items [i].key, loadedData.items [i].value);    
            }

            Debug.Log ("Data loaded, dictionary contains: " + localizedText.Count + " entries");
            InstructionsTextBehavior.instance.ShowTextFromKey("idle");
            //activate/deactivate clip tracks depending on if leader or follower
            _englishTrack.muted = fileName != "lng_en.json";
            _germanTrack.muted = fileName != "lng_de.json";
        } 
        else 
        {
            Debug.LogError ("Cannot find file!");
        }
        
        if (resend) OscManager.instance.SendLanguageChange(fileName);
    }

    public string GetLocalizedValue(string key)
    {
        string result = missingTextString;
        if (localizedText != null && localizedText.ContainsKey (key)) 
        {
            result = localizedText [key];
        } else Debug.Log("key not found or file not loaded yet");
        
        return result;
    }

}
