using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class ConsentUI : MonoBehaviour
{
    [SerializeField] private GameObject _yesButton;
    [SerializeField] private GameObject _noButton;
    [SerializeField] private GameObject _textPanel;
    [SerializeField] private Text _text;
    
    public void ReadyToShowQuestionnaire()
    {
        Show(false);
    }
    
    public void ConsentButtonPressed()
    {
        _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "waitForOther";
    }
    
    public void Show(bool show)
    {
        _yesButton.gameObject.SetActive(show); 
        _noButton.gameObject.SetActive(show);
        if (show) _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "consent"; 
        _textPanel.GetComponent<PanelDimmer>().Show(show, 1f);
    }
}
