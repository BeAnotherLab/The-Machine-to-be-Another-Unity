using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public class ExperimentSettingsGUI : CameraDropdownSelector
{

    public static ExperimentSettingsGUI instance;
    
    //Experiment settings
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private Dropdown _experimentCameraDropdown;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;
    [SerializeField] private GameObject _subjectExistingErrorMessage;

    [SerializeField] private Button _startButton;
    
    [SerializeField] private Button _rotateButton;

    private void Awake()
    {
        if (instance == null) instance = this;

        _experimentCameraDropdown.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.cameraID = _experimentCameraDropdown.value;
            VideoCameraManager.instance.SetAVProCamera(_experimentCameraDropdown.value);
            PlayerPrefs.SetInt("CognitiveTestCamera", _experimentCameraDropdown.value);
        });
        
        _startButton.onClick.AddListener(delegate
        {
            if (_conditionDropdown.options[_conditionDropdown.value].text == "Experimental")
                ExperimentManager.instance.conditionType = ConditionType.experimental;
            else
                ExperimentManager.instance.conditionType = ConditionType.control;

            if (_participantDropdown.options[_participantDropdown.value].text == "Leader")
                ExperimentManager.instance.participantType = ParticipantType.leader;
            else
                ExperimentManager.instance.participantType = ParticipantType.follower;
        });

        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    // Start is called before the first frame update
    private void Start()
    {
        SetCameraDropdownOptions(_experimentCameraDropdown);
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
}
