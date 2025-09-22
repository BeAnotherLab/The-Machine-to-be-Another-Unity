using System;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapLanguageButtons : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;
    [SerializeField] private StringGameEvent _languageChangeGameEvent;

    private void Start()
    {
        _languageChangeGameEvent.Raise("Polish");
    }

    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            _buttons.SetActive(false);
        }
    }
}
