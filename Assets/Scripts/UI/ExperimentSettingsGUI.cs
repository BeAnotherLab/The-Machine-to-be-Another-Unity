using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentSettingsGUI : MonoBehaviour
{

    public static ExperimentSettingsGUI instance;
    
    //Experiment settings
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private Dropdown _experimentCameraDropdown;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    
    //Cognitive settings
    [SerializeField] private Dropdown _pronounDropdown;
    [SerializeField] private Dropdown _CognitiveTestCameraDropdown;
    [SerializeField] private Dropdown _directionDropdown;
    [SerializeField] private Toggle _showDummyToggle;
    
    //Swap settings
    [SerializeField] private Text _FPSText;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _startButton;
    
    private bool _monitorGuiEnabled;
    private float _deltaTime = 0.0f;

    private void Awake()
    {
        if (instance == null) instance = this;        
        
        _CognitiveTestCameraDropdown.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.cameraID = _CognitiveTestCameraDropdown.value;
        });

        _startButton.onClick.AddListener(delegate
        {
            CognitiveTestManager.instance.StartInstructions(
                _pronounDropdown.options[_pronounDropdown.value].text, 
                _subjectIDInputField.text, 
                _directionDropdown.options[_directionDropdown.value].text);

            if (_conditionDropdown.options[_conditionDropdown.value].text == "Experimental")
                ExperimentManager.instance.conditionType = ConditionType.experimental;
            else
                ExperimentManager.instance.conditionType = ConditionType.control;

            if (_participantDropdown.options[_participantDropdown.value].text == "Leader")
                ExperimentManager.instance.participantType = ParticipantType.leader;
            else
                ExperimentManager.instance.participantType = ParticipantType.follower;
        });
        
        _showDummyToggle.onValueChanged.AddListener(delegate(bool value){
            ShowDummy.instance.Show(value);
            _rotateButton.interactable = !value;
            VideoFeed.instance.gameObject.SetActive(!value);
            _directionDropdown.interactable = !value;
        });
        
        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
        
        VideoFeed.instance.OnWebCamConnected += SetCameraDropdownOptions;
    }

    // Start is called before the first frame update
    private void Start()
    {
        SetCameraDropdownOptions();
    }

    // Update is called once per frame
    private void Update()
    {
        ShowFPS();
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
    
    public void SetCameraDropdownOptions()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        _CognitiveTestCameraDropdown.options.Clear();
        
        foreach (WebCamDevice device in devices)
        {
            _CognitiveTestCameraDropdown.options.Add(new Dropdown.OptionData() { text = device.name });
        }
        _CognitiveTestCameraDropdown.value = PlayerPrefs.GetInt("cameraID");
        _CognitiveTestCameraDropdown.RefreshShownValue();
    }

    
    private void ShowFPS()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        int w = Screen.width, h = Screen.height;
        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        _FPSText.text = text;
    }
}
