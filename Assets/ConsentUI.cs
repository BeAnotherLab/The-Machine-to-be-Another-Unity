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
    
    [SerializeField] private UserStateVariable _previousOtherState;

    public void SelfStateChanged(UserState selfState)
    {
        //if self is now ready to start 
        if (selfState == UserState.readyToStart)
        {
            Show(true);
        } 
        else if (selfState == UserState.headsetOff)
        {
            Show(false);
        }
    }

    public void OtherStateChanged(UserState otherState)
    {
        if(otherState == UserState.headsetOff && _previousOtherState == UserState.readyToStart) Show(false);
    }
    
    public void ReadyToShowQuestionnaire()
    {
        Show(false);
    }
    
    
    public void ConsentButtonPressed()
    {
        _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "waitForOther";
    }
    
    private void Show(bool show)
    {
        _yesButton.gameObject.SetActive(show); 
        _noButton.gameObject.SetActive(show);
        if (show) _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "consent"; 
        _textPanel.GetComponent<PanelDimmer>().Show(show, 1f);
    }
}
