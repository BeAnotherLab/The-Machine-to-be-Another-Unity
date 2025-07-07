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

    [SerializeField] private Dropdown _timelineDropdown;
    [SerializeField] private GameObject _panel;
    [SerializeField] private IPInputField _ipInputField;
    [SerializeField] private Toggle _serialControlToggle;
    
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Button _resetYawButton;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Text _exposureText;
    [SerializeField] private Toggle _repeaterToggle;
    
    private bool _oculusGuiEnabled;
    private float _deltaTime = 0.0f;
 
    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        
        _dimButton.onClick.AddListener(delegate { VideoFeed.instance.ToggleDim(); });
        
        _cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        
        _repeaterToggle.onValueChanged.AddListener(delegate { OscManager.instance.SetRepeater(_repeaterToggle.isOn); });

        _serialControlToggle.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetSerialControlComputer(_serialControlToggle.isOn); });
        
        _timelineDropdown.onValueChanged.AddListener(delegate(int val) { StatusManager.instance.SetInstructionsTimeline(val); });
        
        _resetYawButton.onClick.AddListener(delegate { VideoFeed.instance.RecenterPose(); });
        
        _exposureSlider.onValueChanged.AddListener(delegate(float value)
        {
            ExposureValueChanged((int) value);
            PlayerPrefs.SetInt("exposure", (int) value);
            _exposureText.text = "Exposure : " + value;
        });
        
        _rotateCameraButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    // Use this for initialization
    private void Start()
    {        
        _repeaterToggle.isOn = PlayerPrefs.GetInt("repeater") == 1; 
        _serialControlToggle.isOn = PlayerPrefs.GetInt("serialControlOn") == 1;
        
        if (PlayerPrefs.GetInt("exposure", 1) != 1)
        {
            _exposureSlider.value = PlayerPrefs.GetInt("exposure");
            _exposureText.text = "Exposure : " + _exposureSlider.value;
        }
        
        OSCUtilities.GetLocalHost(); //TODO remove?
    }

    private void Update()
    {     
        //TODO move out of settings GUI
        if (Input.GetKeyDown("m")) ToggleDisplay();
    }

    #endregion

    #region Public Methods

    public void SetSwapMode(bool withArduino = false) 
    {
        _serialControlToggle.gameObject.SetActive(withArduino);
        
        //show two way swap related networking GUI
        _repeaterToggle.gameObject.SetActive(true);
        _ipInputField.gameObject.SetActive(true);
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

    #endregion
}
