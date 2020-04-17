using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public class ExperimentSettingsGUI : MonoBehaviour
{

    public static ExperimentSettingsGUI instance;
    
    //Experiment settings
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;

    [SerializeField] private Button _startButton;
    
    [SerializeField] private Button _rotateButton;
    
    [SerializeField] private GameObject _subjectExistingErrorMessage;

    private void Awake()
    {
        if (instance == null) instance = this;

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
            
            ExperimentManager.instance.StartExperiment(_conditionDropdown.options[_conditionDropdown.value].text == "Familiarization");
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

}
