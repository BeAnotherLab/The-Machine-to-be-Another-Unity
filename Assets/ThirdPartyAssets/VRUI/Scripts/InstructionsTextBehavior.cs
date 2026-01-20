using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsTextBehavior : MonoBehaviour
{
    [SerializeField] private GameObject _textGameObject;
    [SerializeField] private Translations _translations;

    [SerializeField] private string _textKey;
    
    #region  Public methods
   
    private void ShowInstructionText(bool show, string text = "")
    {
        GetComponent<PanelDimmer>().Show(show);
        if (show) _textGameObject.GetComponent<TMP_Text>().text = text; //give feedback
    }

    public void ShowInstructionsText(string text) //called by timeline / sequencer
    {
        _textGameObject.GetComponent<TMP_Text>().text = text; //give feedback
    }

    public void ShowTextFromKey(string key) //set text through localized text translations scriptable object
    {
        _textKey = key;
        
        GetComponent<PanelDimmer>().Show();

        if (_translations.Value != null && _translations.Value.TryGetValue(key, out string translatedText))
        {
            _textGameObject.GetComponent<TMP_Text>().text = translatedText; //give feedback
        }
        else
        {
            Debug.LogWarning($"Missing translation for key: {key}");
        }
    }

    public void LanguageChange(string languageCode)
    {
        if (_translations.Value != null && _translations.Value.TryGetValue(_textKey, out string translatedText))
        {
            _textGameObject.GetComponent<TMP_Text>().text = translatedText; //give feedback
        }
        else
        {
            Debug.LogWarning($"Missing translation for key: {_textKey}");
        }
    }
    
    public void ShowTextFromKey(string text, int time)
    {
        StartCoroutine(TimedTextCoroutineFromKey(text, time));
    }
    
    public void ShowInstructionTextFromKey(string text, int time)
    {
        StartCoroutine(TimedTextCoroutine(text, time));
    }
    
    #endregion
    
    #region Private Methods
    
    private IEnumerator TimedTextCoroutine(string text, int time)
    {
        ShowTextFromKey(text);
        yield return new WaitForSeconds(time);
        ShowInstructionText(false);       
    }
    
    private IEnumerator TimedTextCoroutineFromKey(string key, int time)
    {
        ShowTextFromKey(key);
        yield return new WaitForSeconds(time);
        ShowInstructionText(false);       
    }
    
    #endregion
}