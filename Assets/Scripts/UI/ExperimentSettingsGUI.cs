using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public class ExperimentSettingsGUI : MonoBehaviour
{
    //TODO when setting familiarization, disable ip address and participant
    public static ExperimentSettingsGUI instance;
    
    [Header("Experiment Data")]
    [SerializeField] private InputField _subjectInputField;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;
    [SerializeField] private Dropdown _taskCounterbalancingDropdown;
    [SerializeField] private Dropdown _threatCounterbalancingDropdown;

    [Header("Other Data")]
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _yawResetButton;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    [SerializeField] private GameObject _videoNotFoundErrorMessage;
    [SerializeField] private List<String> tasks = new List<string>();
    [SerializeField] private ExperimentData _experimentData;
    
    private void Awake()
    {
        if (instance == null) instance = this;

        _subjectInputField.onEndEdit.AddListener( delegate {
            FamiliarizationManager.instance.SetSubjectID(_subjectInputField.text);
            VideoFeed.instance.IsEditingText(false);
        });

        _subjectInputField.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.IsEditingText(true);
        });
        
        _startButton.onClick.AddListener(delegate
        {
            FamiliarizationManager.instance.SelectThreatOrder(_threatCounterbalancingDropdown.options[_threatCounterbalancingDropdown.value].text);
            FamiliarizationManager.instance.SelectTaskOrder(_taskCounterbalancingDropdown.options[_taskCounterbalancingDropdown.value].text);
            FamiliarizationManager.instance.StartExperiment(
                _conditionDropdown.options[_conditionDropdown.value].text,
                _participantDropdown.options[_participantDropdown.value].text,
                _subjectInputField.text);
        });

        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
        
        _yawResetButton.onClick.AddListener(delegate { VideoFeed.instance.RecenterPose(); });
    }

    private void Start()
    {
        //initialize GUI values with experiment data values
        _subjectInputField.text = _experimentData.subjectID;
        AssignDropdownValue(_experimentData.conditionType.ToString(), _conditionDropdown);
        AssignDropdownValue(_experimentData.participantType.ToString(), _participantDropdown);
        AssignDropdownValue(_experimentData.taskOrder.ToString(), _taskCounterbalancingDropdown);
        AssignDropdownValue(_experimentData.threatOrder, _threatCounterbalancingDropdown); 
        
        List<string> _dropDownOptions = new List<string>();

        _taskCounterbalancingDropdown.AddOptions(_dropDownOptions);    
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

    private void AssignDropdownValue(string value, Dropdown dropdown)
    {
        // returns a list of the text properties of the options
        var listAvailableStrings = dropdown.options.Select(option => option.text).ToList();

        // returns the index of the given string
        dropdown.value = listAvailableStrings.IndexOf(value);
    }
}
