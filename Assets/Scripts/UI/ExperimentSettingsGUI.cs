using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public class ExperimentSettingsGUI : MonoBehaviour
{
    //TODO when setting familiarization, disable ip address and participant
    public static ExperimentSettingsGUI instance;
    public bool subjectIDValidated;
    
    //Experiment settings
    [SerializeField] private InputField _subjectInputField;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    [SerializeField] private GameObject _videoNotFoundErrorMessage;
    [SerializeField] private Dropdown _directionDropdown;
    
    private void Awake()
    {
        if (instance == null) instance = this;

        _subjectInputField.onEndEdit.AddListener( delegate {
            if(_subjectInputField.text != "") subjectIDValidated = true;
            FamiliarizationManager.instance.SetSubjectID(_subjectInputField.text);
        });
        
        _startButton.onClick.AddListener(delegate
        {
            FamiliarizationManager.instance.StartExperiment(
                _conditionDropdown.options[_conditionDropdown.value].text,
                _participantDropdown.options[_participantDropdown.value].text,
                _subjectInputField.text, 
                _directionDropdown.options[_directionDropdown.value].text);
        });

        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    public void NotifyVideoNotFoundError()
    {
        StartCoroutine(ShowAndHideVideoNotFoundError());
    }
    
    public void ShowExistingSubjectIDError()
    {
        StartCoroutine(ShowAndHideExistingSubjectIDError());
    }
    
    private IEnumerator ShowAndHideExistingSubjectIDError()
    {
        _subjectExistingErrorMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        _subjectExistingErrorMessage.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowAndHideVideoNotFoundError()
    {
        _videoNotFoundErrorMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        _videoNotFoundErrorMessage.gameObject.SetActive(false);
    }

}
