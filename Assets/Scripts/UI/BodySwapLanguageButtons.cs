using System;
using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapLanguageButtons : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;
    [SerializeField] private StringGameEvent _languageChangeGameEvent;
    

    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart) //hide language buttons when user is ready
        {
            _buttons.SetActive(false);
        }
    }
    
}
