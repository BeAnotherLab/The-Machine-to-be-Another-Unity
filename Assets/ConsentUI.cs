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
        if ( selfState == UserState.readyToStart)
        {
            _yesButton.gameObject.SetActive(true); 
            _noButton.gameObject.SetActive(true);
            _textPanel.GetComponent<PanelDimmer>().Show(true, 0.6f);    
        } 
    }

    public void ConsentButtonPressed()
    {
        _text.text = "Wait for a moment...";
    }
}
