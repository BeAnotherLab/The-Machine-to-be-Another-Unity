using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class ConsentUI : MonoBehaviour
{
    [SerializeField] private GameObject _yesButton;
    [SerializeField] private GameObject _noButton;
    [SerializeField] private GameObject _textPanel;
    [SerializeField] private Text _text;
    
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
        
    }
    
    public void ReadyToShowQuestionnaire()
    {
        Show(false);
    }
    
    
    public void ConsentButtonPressed()
    {
        _text.text = "Wait for a moment...";
    }
    
    private void Show(bool show)
    {
        _yesButton.gameObject.SetActive(show); 
        _noButton.gameObject.SetActive(show);
        _textPanel.GetComponent<PanelDimmer>().Show(show, 1f);
    }
}
