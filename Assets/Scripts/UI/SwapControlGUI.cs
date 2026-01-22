using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SwapControlGUI : MonoBehaviour 
{
    public delegate void OnAudioButtonPressed(string key);
    public static OnAudioButtonPressed AudioButtonPressed;
    
    [SerializeField] private IntGameEvent _buttonPressedEvent;
    [SerializeField] private GameObject _controlPanel;

    private Button _audioButtons;

    public void ButtonPressed(string key) //TODO make this repeat on the other side if repeater? or regardless?
    {
        AudioButtonPressed(key); 
    }
    
}