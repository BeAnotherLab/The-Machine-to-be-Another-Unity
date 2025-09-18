using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SwapControlGUI : MonoBehaviour  
{
    public delegate void OnAudioButtonPressed(int i);
    public static OnAudioButtonPressed AudioButtonPressed;
    
    public delegate void OnRecenterPoserButtonPressed();
    public static OnRecenterPoserButtonPressed RecenterPoserButtonPressed;
    
    public delegate void OnDimButtonOn(bool dimOn);
    public static OnDimButtonOn DimButtonOn;
    
    [SerializeField] private StringGameEvent _languagechangedEvent;
    [SerializeField] private IntGameEvent _buttonPressedEvent;
    [SerializeField] private BoolGameEvent _dimButtonPressedEvent;
    [SerializeField] private GameEvent _calibratebuttonPressedEvent;
    [SerializeField] private GameObject _controlPanel;

    private Button _audioButtons;

    public void ButtonPressed(int id)
    {
        AudioButtonPressed(id); //TODO remove redundancy with repeater underneath?
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _buttonPressedEvent.Raise(id);
    }

    public void CalibrateButtonPressed()
    {
        RecenterPoserButtonPressed(); //TODO remove redundancy with repeater underneath?
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _calibratebuttonPressedEvent.Raise();
    }

    public void DimButtonPressed(bool dimOn)
    {
        DimButtonOn(dimOn); //TODO remove redundancy with repeater underneath?
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _dimButtonPressedEvent.Raise(dimOn);
    }
    
    public void LanguageChanged(string language)
    {
        _languagechangedEvent.Raise(language);
    }
    
}