using System;
using System.IO;
using UnityEngine;

public class LanguageButtonsTextureLoader : MonoBehaviour
{
    [SerializeField] private GameObject[] _languageButtons;

    private int _loadedFlags;

    private void OnEnable()
    {
        DataLoader.LoadLanguageButtonTexture += LoadFlagTexture;
    }

    private void OnDisable()
    {
        DataLoader.LoadLanguageButtonTexture -= LoadFlagTexture;
    }

    private void LoadFlagTexture(string language)
    {
        if (_loadedFlags < 4)
        {
            LoadFlagTexture(_languageButtons[_loadedFlags], language);
            _loadedFlags++;    
        }
    }
    
    private void LoadFlagTexture(GameObject buttonObj, string languageCode)
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
        buttonObj.SetActive(true);
    }
}