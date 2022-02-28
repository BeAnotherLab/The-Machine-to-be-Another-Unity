using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using extOSC;

public class SettingsGUI : MonoBehaviour
{
    #region Public fields

    public static SettingsGUI instance;
    public delegate void OnExposureValueChanged(int value);
    public static OnExposureValueChanged ExposureValueChanged;
    
    #endregion

    #region Private Fields

    [SerializeField] private Dropdown _swapModeDropdown;
    [SerializeField] private Dropdown _timelineDropdown;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Slider _pitchSlider, _yawSlider, _rollSlider, _zoomSlider;
    [SerializeField] private IPInputField _ipInputField;
    [SerializeField] private Toggle _serialControlToggle;
    
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Button _headTrackingOnButton;
    [SerializeField] private Button _resetYawButton;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Text _exposureText;
    [SerializeField] private Toggle _repeaterToggle;
    //[SerializeField] private Text _controlsText;
    
    private GameObject _mainCamera;
    private bool _oculusGuiEnabled;
    private float _deltaTime = 0.0f;
 
    [SerializeField] private bool serialDebug;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        //objects in the scene
        _mainCamera = GameObject.Find("Main Camera");
        
        _dimButton.onClick.AddListener(delegate { VideoFeed.instance.ToggleDim(); });
        
        _cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        
        _repeaterToggle.onValueChanged.AddListener(delegate { OscManager.instance.SetRepeater(_repeaterToggle.isOn); });

        _serialControlToggle.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetSerialControlComputer(_serialControlToggle.isOn); });
        
        _timelineDropdown.onValueChanged.AddListener(delegate(int val) { StatusManager.instance.SetInstructionsTimeline(val); });
        
        //_controlsText.text = _controlsText.text + "\n \nlocal IP adress : " + OSCUtilities.GetLocalHost();

        //Assign servos control buttons handlers
        _pitchSlider.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetPitch(_pitchSlider.value); });
        _yawSlider.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetYaw(_yawSlider.value); });
        _zoomSlider.onValueChanged.AddListener(delegate { VideoFeed.instance.SetZoom(_zoomSlider.value); });
        
        _headTrackingOnButton.onClick.AddListener(delegate { VideoFeed.instance.SwitchHeadtracking(); });
        _resetYawButton.onClick.AddListener(delegate { VideoFeed.instance.RecenterPose(); });
        
        _exposureSlider.onValueChanged.AddListener(delegate(float value)
        {
            ExposureValueChanged((int) value);
            PlayerPrefs.SetInt("exposure", (int) value);
            _exposureText.text = "Exposure : " + value;
        });
        
        //Assign swap mode dropdown handler
        _swapModeDropdown.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode( (SwapModeManager.SwapModes) _swapModeDropdown.value); });
        
        _rotateCameraButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    // Use this for initialization
    private void Start()
    {        
        SetSwapModeDropdownOptions();

        _zoomSlider.value = PlayerPrefs.GetFloat("zoom", 39.5f);

        if (PlayerPrefs.GetInt("repeater") == 1) _repeaterToggle.isOn = true;
        else                                     _repeaterToggle.isOn = false;

        if (PlayerPrefs.GetInt("serialControlOn") == 1) _serialControlToggle.isOn = true;
        else _serialControlToggle.isOn = false;             
        
        if (PlayerPrefs.GetInt("exposure", 1) != 1)
        {
            var savedValue = PlayerPrefs.GetInt("exposure");
            if (savedValue == _exposureSlider.value) _exposureSlider.onValueChanged.Invoke(_exposureSlider.value);

            _exposureSlider.value = savedValue;
            _exposureText.text = "Exposure : " + _exposureSlider.value;
        }
        
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
        AudioManager.instance.language = language;

        SetLanguageText(language);

        PlayerPrefs.SetInt("language", language);
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

    public void ToggleDebugDisplayGUI()
    {
        DisplayManager.instance.ToggleDisplayMode();
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
