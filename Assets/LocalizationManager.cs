using System;
using ScriptableObjectArchitecture;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    [SerializeField] private StringVariable _currentLanguage;

    private bool firstLanguageLoaded;
    
    private void OnEnable()
    {
        DataLoader.LoadLanguage += LanguageLoaded;
    }

    private void OnDisable()
    {
        DataLoader.LoadLanguage -= LanguageLoaded;
    }

    public void SetLanguage(string language)
    {
        _currentLanguage.Value = language;
    }

    private void LanguageLoaded(string language)
    {
        if (!firstLanguageLoaded) SetLanguage(language);
        firstLanguageLoaded = true;
    }
}
