using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CustomSwapControlGUI : MonoBehaviour
{
    [SerializeField] private StringGameEvent _languagechangedEvent;
    [SerializeField] private IntGameEvent _buttonPressedEvent;
    [SerializeField] private GameEvent _startInstructionsButtonPressed;
    [SerializeField] private GameEvent _stopInstructionsButtonPressed;
    [SerializeField] private GameEvent _CalibratebuttonPressedEvent;
    //[SerializeField] private GameObject _controlPanel;

    private Button _audioButtons;

    public void ButtonPressed(int id)
    {
        AudioManager.instance.PlaySound(id);
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _buttonPressedEvent.Raise(id);
    }

    public void CalibrateButtonPressed()
    {
        VideoFeed.instance.RecenterPose(); 
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _CalibratebuttonPressedEvent.Raise();
    }

    public void StartInstructionsButtonPressed(bool start)
    {
        VideoFeed.instance.Dim(!start);
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
        {
            if (start) _startInstructionsButtonPressed.Raise();
            else _stopInstructionsButtonPressed.Raise();
        }
    }
    
    public void LanguageChanged(string language)
    {
        _languagechangedEvent.Raise(language);
    }
}