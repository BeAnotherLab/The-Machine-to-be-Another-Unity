using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class ConsentUI : MonoBehaviour
{
    [SerializeField] private UserStateVariable _selfState;
    [SerializeField] private UserStateVariable _otherState;

    [SerializeField] private BoolVariable _selfConsented;
    [SerializeField] private BoolVariable _otherConsented;

    [SerializeField] private GameObject _yesButton;
    [SerializeField] private GameObject _noButton;

    [SerializeField] private GameObject _textPanel;
    
    public void SelfStateChanged(UserState selfState)
    {
        //if self is now ready to start and other consented or other did not consent yet, ask for consent
        if ( selfState == UserState.readyToStart && (_otherConsented.Value || UserStateOperations.IsBeforeConsent(_otherState)))
        {
            _yesButton.gameObject.SetActive(true); 
            _noButton.gameObject.SetActive(true);
            _textPanel.GetComponent<PanelDimmer>().Show(true, 0.6f);    
        } 
    }

}
