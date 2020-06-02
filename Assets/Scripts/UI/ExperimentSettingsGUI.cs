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

    private void Awake()
    {
        if (instance == null) instance = this;

        _subjectInputField.onEndEdit.AddListener( delegate {
            PlayerPrefs.SetString("SubjectID", _subjectInputField.text);
            if(_subjectInputField.text != "") subjectIDValidated = true;
        });
        
        _startButton.onClick.AddListener(delegate
        {
            if (_conditionDropdown.options[_conditionDropdown.value].text == "Experimental")
                ExperimentManager.instance.condition = ConditionType.experimental;
            else if (_conditionDropdown.options[_conditionDropdown.value].text == "Control")
                ExperimentManager.instance.condition = ConditionType.control;
            else if (_conditionDropdown.options[_conditionDropdown.value].text == "Familiarization")
                ExperimentManager.instance.condition = ConditionType.familiarization;

            if (_participantDropdown.options[_participantDropdown.value].text == "Leader")
                ExperimentManager.instance.participant = ParticipantType.leader;
            else
                ExperimentManager.instance.participant = ParticipantType.follower;
            
            //check if follower control video file could be found before starting
            var currentSubjectID = PlayerPrefs.GetString("SubjectID");
            if (ExperimentManager.instance.condition == ConditionType.control &&
                ExperimentManager.instance.participant == ParticipantType.follower && 
                !File.Exists(PlayerPrefs.GetString("VideoCapturePath" + currentSubjectID)))
            {
                StartCoroutine(ShowAndHideVideoNotFoundError());
            }
            else
            {
                PlayerPrefs.SetString("SubjectID", _subjectInputField.text);
                ExperimentManager.instance.StartExperiment();
            }
            
        });

        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
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
