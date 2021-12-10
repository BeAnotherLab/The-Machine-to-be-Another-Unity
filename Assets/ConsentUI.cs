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
    
    public void SelfStateChanged(UserState otherState)
    {
        //if other consented or other did not consent yet, ask for consent
        if (_otherConsented.Value || UserStateOperations.IsBeforeConsent(otherState))
        {
            _yesButton.gameObject.SetActive(true); 
            _noButton.gameObject.SetActive(true);
            GetComponent<PanelDimmer>().Show(true, 0.6f);    
        } 
    }

}
