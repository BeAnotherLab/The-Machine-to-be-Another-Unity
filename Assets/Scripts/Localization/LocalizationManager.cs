using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour {

    public static LocalizationManager instance;

    [SerializeField] private string[] localizationTexts; //TODO autoload
    
    private Dictionary<string, string> localizedText;
    private bool isReady = false;
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
        LoadLocalizedText(localizationTexts[0]);
    }

    private void Update()
    {
        if (Input.GetKeyDown("z")) LoadLocalizedText(localizationTexts[0]);
        if (Input.GetKeyDown("x")) LoadLocalizedText(localizationTexts[1]);
        if (Input.GetKeyDown("c")) LoadLocalizedText(localizationTexts[2]);
        if (Input.GetKeyDown("v")) LoadLocalizedText(localizationTexts[3]);
    }

    public void LoadLocalizedText(string fileName, bool fromOSC = false)
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
        } else 
        {
            Debug.LogError ("Cannot find file!");
        }

        isReady = true;
        
        if (!fromOSC) OscManager.instance.SendLanguageChange(fileName + ".json");
    }

    public string GetLocalizedValue(string key)
    {
        string result = missingTextString;
        if (localizedText.ContainsKey (key)) 
        {
            result = localizedText [key];
        } else Debug.Log("key not found");

        return result;

    }

}
