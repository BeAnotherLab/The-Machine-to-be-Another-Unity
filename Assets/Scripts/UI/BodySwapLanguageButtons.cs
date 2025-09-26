using System;
using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapLanguageButtons : MonoBehaviour
{
    [SerializeField] private GameObject _buttons;
    [SerializeField] private StringGameEvent _languageChangeGameEvent;

    private void Start()
    {
        StartCoroutine(WaitAndSetPolish());
    }

    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            _buttons.SetActive(false);
        }
    }

    private IEnumerator WaitAndSetPolish()
    {
        yield return new WaitForSeconds(1f);
        _languageChangeGameEvent.Raise("Polish");
    }
}
