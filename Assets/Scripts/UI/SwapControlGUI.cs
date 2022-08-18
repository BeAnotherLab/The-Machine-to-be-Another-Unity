using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SwapControlGUI : MonoBehaviour
{
    [SerializeField] private StringGameEvent _languagechangedEvent;
    [SerializeField] private IntGameEvent _buttonPressedEvent;
    [SerializeField] private BoolGameEvent _dimButtonPressedEvent;
    [FormerlySerializedAs("_calibratebuttonPressedEvent")] [SerializeField] private GameEvent _CalibratebuttonPressedEvent;
    [SerializeField] private GameObject _controlPanel;

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
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _buttonPressedEvent.Raise(id);
    }

    public void CalibrateButtonPressed()
    {
        VideoFeed.instance.RecenterPose(); 
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _CalibratebuttonPressedEvent.Raise();
    }

    public void DimButtonPressed(bool dimOn)
    {
        VideoFeed.instance.Dim(dimOn);
        if (PlayerPrefs.GetInt("repeater", 0) == 1)
            _dimButtonPressedEvent.Raise(dimOn);
    }
    
    public void LanguageChanged(string language)
    {
        _languagechangedEvent.Raise(language);
    }
    
    private void SwapModeChanged(SwapModeManager.SwapModes swapMode)
    {
        if (swapMode == SwapModeManager.SwapModes.CURTAIN_MANUAL_SWAP && 
            PlayerPrefs.GetInt("repeater") == 1 &&
            PlayerPrefs.GetInt("serialControlOn") == 1) 
        {
            _controlPanel.SetActive(true);
        } else 
        {
            _controlPanel.SetActive(false);
        }
    } 
}