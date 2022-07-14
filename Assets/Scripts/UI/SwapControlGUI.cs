﻿using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class SwapControlGUI : MonoBehaviour
{
    [SerializeField] private StringGameEvent _languagechangedEvent;
    [SerializeField] private IntGameEvent _buttonPressedEvent;
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

    private void Start()
    {
        _curtainControlButtons.gameObject.SetActive(PlayerPrefs.GetInt("repeater") == 1 &&
                                                    PlayerPrefs.GetInt("serialControlOn") == 1);
    }

    public void ButtonPressed(int id)
    {
        AudioManager.instance.PlaySound(id);
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _buttonPressedEvent.Raise(id);
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