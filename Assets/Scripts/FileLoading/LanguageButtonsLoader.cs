using System;
using System.IO;
using UnityEngine;
using VRStandardAssets.Menu;

public class LanguageButtonsLoader : MonoBehaviour //TODO better move that so buttons self manage
{
    [SerializeField] private GameObject[] _languageButtons;

    private int _loadedFlags;

    private void OnEnable()
    {
        DataLoader.LoadLanguage += LoadLanguageOnButton;
    }

    private void OnDisable()
    {
        DataLoader.LoadLanguage -= LoadLanguageOnButton;
    }

    private void LoadLanguageOnButton(string language)
    {
        if (_loadedFlags < 4)
        {
            LoadLanguageOnButton(_languageButtons[_loadedFlags], language);
            _loadedFlags++;    
        }
    }
    
    private void LoadLanguageOnButton(GameObject buttonObj, string languageCode)
    {
        string filename = $"flag_{languageCode}.png";
        string path = ContentPath.Static(filename);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Flag texture not found: {path}");
            return;
        }

        byte[] data = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data);
        tex.Apply();

        MeshRenderer renderer = buttonObj.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.material.mainTexture = tex;
        else Debug.LogWarning($"MeshRenderer not found on: {buttonObj.name}");
        buttonObj.GetComponent<LanguageButton>().SetLanguage(languageCode);
        buttonObj.SetActive(true);
    }
}