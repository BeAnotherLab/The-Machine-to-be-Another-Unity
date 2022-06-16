using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class SwapControlGUI : MonoBehaviour
{
    [SerializeField] private StringGameEvent _languagechangedEvent;
    [SerializeField] private GameObject _curtainControlButtons;

    private Button _audioButtons;

    private void OnEnable()
    {
        SwapModeManager.SwapModeChanged += SwapModeChanged;
    }

    private void OnDisable()
    {
        SwapModeManager.SwapModeChanged -= SwapModeChanged;
    }

    public void ButtonPressed(int id)
    {
        AudioManager.instance.PlaySound(id);
    }

    public void LanguageChanged(string language)
    {
        _languagechangedEvent.Raise(language);
    }
    
    private void SwapModeChanged(SwapModeManager.SwapModes swapMode)
    {
        if (swapMode == SwapModeManager.SwapModes.CURTAIN_MANUAL_SWAP) 
        {
            _curtainControlButtons.SetActive(true);
        } else 
        {
            _curtainControlButtons.SetActive(false); 
        }
    } 
}