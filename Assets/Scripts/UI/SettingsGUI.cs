using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using extOSC;
using VRTK;

public class SettingsGUI : MonoBehaviour
{
    #region Public field

    public static SettingsGUI instance;

    #endregion

    #region Private Fields

    [SerializeField] private Dropdown _swapModeDropdown;
    [SerializeField] private Dropdown _timelineDropdown;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Slider _pitchSlider, _yawSlider, _rollSlider, _zoomSlider;
    [SerializeField] private IPInputField _ipInputField;
    [SerializeField] private Toggle _serialControlToggle;
    
    [SerializeField] private InputField _cameraNameInputField;
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Button _headTrackingOnButton;
    [SerializeField] private Toggle _repeaterToggle;
    [SerializeField] private Text _controlsText;
    [SerializeField] private Text _languageText;
    
    private GameObject _mainCamera;
    private bool _oculusGuiEnabled;
    private float _deltaTime = 0.0f;
 
    [SerializeField] private bool serialDebug;

    #endregion

    #region MonoBehaviour Methods

    protected virtual void OnEnable() {
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }
    protected virtual void OnDestroy() {
        VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
    }
    
    private void Awake()
    {
        if (instance == null) instance = this;

        if(VRTK_SDKManager.instance != null)
            VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange (this);

        _cameraNameInputField.onEndEdit.AddListener(delegate(string arg0)
        {
            VideoCameraManager.instance.SetCameraName(_cameraNameInputField.text);
            PlayerPrefs.SetString("CameraName", _cameraNameInputField.text);
        });
        
        _dimButton.onClick.AddListener(delegate { VideoFeed.instance.ToggleDim(); });
        
        _cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        
        _repeaterToggle.onValueChanged.AddListener(delegate { OscManager.instance.SetRepeater(_repeaterToggle.isOn); });

        _serialControlToggle.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetSerialControlComputer(_serialControlToggle.isOn); });
        
        _timelineDropdown.onValueChanged.AddListener(delegate(int val) { StatusManager.instance.SetInstructionsTimeline(val); });
        
        _controlsText.text = _controlsText.text + "\n \nlocal IP adress : " + OSCUtilities.GetLocalHost();

        //Assign servos control buttons handlers
        _pitchSlider.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetPitch(_pitchSlider.value); });
        _yawSlider.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetYaw(_yawSlider.value); });
        _zoomSlider.onValueChanged.AddListener(delegate { VideoFeed.instance.SetZoom(_zoomSlider.value); });
        
        _headTrackingOnButton.onClick.AddListener(delegate { VideoFeed.instance.SwitchHeadtracking(); });

        //Assign swap mode dropdown handler
        _swapModeDropdown.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode( (SwapModeManager.SwapModes) _swapModeDropdown.value); });
        
        _rotateCameraButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    // Use this for initialization
    private void Start()
    {        
        SetSwapModeDropdownOptions();

        _cameraNameInputField.text = PlayerPrefs.GetString("CameraName");
        
        _zoomSlider.value = PlayerPrefs.GetFloat("zoom", 39.5f);

        if (PlayerPrefs.GetInt("repeater") == 1) _repeaterToggle.isOn = true;
        else                                     _repeaterToggle.isOn = false;

        if (PlayerPrefs.GetInt("serialControlOn") == 1) _serialControlToggle.isOn = true;
        else _serialControlToggle.isOn = false;             
        
        OSCUtilities.GetLocalHost();

        SetLanguageText(PlayerPrefs.GetInt("language"));
    }

    private void Update()
    {     
        //TODO move out of settings GUI
        if (Input.GetKeyDown("m")) ToggleDisplay();

        if (Input.GetKeyDown("f"))
        {
            VideoFeed.instance.FlipHorizontal();
        }
        if (VideoFeed.instance.useHeadTracking)
        {
            Vector3 pitchYawRoll = utilities.toEulerAngles(_mainCamera.transform.rotation);

            _rollSlider.value = pitchYawRoll.x;
            _yawSlider.value = 90 - pitchYawRoll.y;
            _pitchSlider.value = pitchYawRoll.z + 90;
            _zoomSlider.value = VideoFeed.instance.zoom;
        }

    }

    #endregion

    #region Public Methods

    public void SetLanguage(int language)
    {
        AudioPlayer.instance.language = language;

        SetLanguageText(language);

        PlayerPrefs.SetInt("language", language);
    }
    
    public void SetSwapMode(SwapModeManager.SwapModes mode) //this is called from swap mode manager since we cannot change dropdown value without triggering event
    {
        Debug.Log("set mode " + mode);
        Debug.Log("set mode (int value)" + (int) mode);
        _swapModeDropdown.value = (int) mode;
        _swapModeDropdown.RefreshShownValue();
    }
    
    public void SetMonitorGuiEnabled(bool show)
    {
       if (show) _panel.GetComponent<CanvasGroup>().alpha = 1f;
       else _panel.GetComponent<CanvasGroup>().alpha = 0f;
    }
                
    public void SetSwapMode(bool withArduino = false) 
    {
        _serialControlToggle.gameObject.SetActive(withArduino);
        
        //show two way swap related networking GUI
        _repeaterToggle.gameObject.SetActive(true);
        _ipInputField.gameObject.SetActive(true);
    }

    public void SetServoMode()
    {
        //hide two way swap related networking GUI
        _ipInputField.gameObject.SetActive(false);
        _repeaterToggle.gameObject.SetActive(false);
    }

    #endregion

    #region Private Methods
    
    private void ToggleDisplay()
    {
        if (_panel.GetComponent<CanvasGroup>().alpha == 0f) _panel.GetComponent<CanvasGroup>().alpha = 1f;
        else _panel.GetComponent<CanvasGroup>().alpha = 0f;
    }
    
    private int GetSerialIndexByOptionName(Dropdown dropDown, string name)
    {
        List<Dropdown.OptionData> list = dropDown.options;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].text.Equals(name)) { return i; }
        }
        return -1;
    }

    private void SetLanguageText(int language)
    {
        string languageString = "English";
        if (language == 1) languageString = "French";
        else if (language == 2) languageString = "Italian";
        _languageText.text = "Language : " + languageString;
    }

    private void SetSwapModeDropdownOptions()
    {
        _swapModeDropdown.options.Add(new Dropdown.OptionData() { text = "Auto Swap"});
        _swapModeDropdown.options.Add(new Dropdown.OptionData() { text = "Manual Swap"});
        _swapModeDropdown.options.Add(new Dropdown.OptionData() { text = "Servo Swap"});

        _swapModeDropdown.value = PlayerPrefs.GetInt("swapMode");
        _swapModeDropdown.RefreshShownValue();
    }
    
    #endregion
}
