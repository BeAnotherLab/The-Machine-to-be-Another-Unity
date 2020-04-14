﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitiveSettingsGUI : CameraDropdownSelector
{
    //Cognitive settings
    [SerializeField] private Dropdown _pronounDropdown;
    [SerializeField] private Dropdown _cognitiveTestCameraDropdown;
    [SerializeField] private Dropdown _directionDropdown;
    [SerializeField] private Toggle _showDummyToggle;
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private Button _rotateButton;

    [SerializeField] private Button _startButton;

    public static CognitiveSettingsGUI instance;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
        
        _cognitiveTestCameraDropdown.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.cameraID = _cognitiveTestCameraDropdown.value;
            CognitiveTestCameraManager.instance.SetAVProCamera(_cognitiveTestCameraDropdown.value);
        });
        
        _showDummyToggle.onValueChanged.AddListener(delegate(bool value){
            ShowDummy.instance.Show(value);
            _rotateButton.interactable = !value;
            VideoFeed.instance.gameObject.SetActive(!value);
            _directionDropdown.interactable = !value;
        });

        _startButton.onClick.AddListener(delegate
        {
            CognitiveTestManager.instance.StartInstructions(
                _pronounDropdown.options[_pronounDropdown.value].text,
                _subjectIDInputField.text,
                _directionDropdown.options[_directionDropdown.value].text);
        });
        
        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
        }

    private void Start()
    {
        SetCameraDropdownOptions(_cognitiveTestCameraDropdown);
    }
}
