using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitiveTestSettingsGUI : MonoBehaviour
{

    public static CognitiveTestSettingsGUI instance;
    
    [SerializeField] private Text _FPSText;
    [SerializeField] private Dropdown _pronounDropdown;
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private Dropdown _cameraDropdown;
    [SerializeField] private Dropdown _directionDropdown;
    [SerializeField] private Toggle _showDummyToggle;
    [SerializeField] private Button _startButton;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    
    private bool _monitorGuiEnabled;
    private float _deltaTime = 0.0f;

    private void Awake()
    {
        if (instance == null) instance = this;        
        
        _cameraDropdown.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.cameraID = _cameraDropdown.value;
        });

        _startButton.onClick.AddListener(delegate
        {
            CognitiveTestManager.instance.StartInstructions(
                _pronounDropdown.options[_pronounDropdown.value].text, 
                _subjectIDInputField.text, 
                _directionDropdown.options[_directionDropdown.value].text);
        });
        
        _showDummyToggle.onValueChanged.AddListener(delegate(bool value){
            ShowDummy.instance.Show(value);
            VideoFeed.instance.gameObject.SetActive(!value);
            _directionDropdown.interactable = !value;
        });
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
    
    private void SetCameraDropdownOptions()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        _cameraDropdown.options.Clear();
        
        foreach (WebCamDevice device in devices)
        {
            _cameraDropdown.options.Add(new Dropdown.OptionData() { text = device.name });
        }
        _cameraDropdown.value = PlayerPrefs.GetInt("cameraID");
        _cameraDropdown.RefreshShownValue();
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
