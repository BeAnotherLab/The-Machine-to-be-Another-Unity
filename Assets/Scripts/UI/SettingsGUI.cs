using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using extOSC;
using ScriptableObjectArchitecture;
using UnityEngine.Serialization;

public class SettingsGUI : MonoBehaviour
{
    #region Public fields

    public delegate void OnExposureValueChanged(int value);
    public static OnExposureValueChanged ExposureValueChanged;

    public delegate void OnToggleDim();
    public static OnToggleDim ToggleDim;

    public delegate void OnRotateCamera();
    public static OnRotateCamera RotateCamera;

    public delegate void OnRecenterPose();
    public static OnRecenterPose RecenterPose;
    
    public delegate void OnSetRepeater(bool on);
    public static OnSetRepeater SetRepeater;
    
    public delegate void OnSetSerialControl(bool on);
    public static OnSetSerialControl SetSerialControl;
    
    public delegate void OnDebugMenuPressed();
    public static OnDebugMenuPressed DebugMenuPressed;
    
    
    
    public delegate void OnRecenterPose();
    public static OnRecenterPose RecenterPose;

    #endregion

    #region Private Fields

    [SerializeField] private GameObject _panel;
    [SerializeField] private IPInputField _ipInputField;
    [SerializeField] private Toggle _serialControlToggle;
    
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Button _resetYawButton;
    [SerializeField] private Button _debugUIButton;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Text _exposureText;
    [SerializeField] private Toggle _repeaterToggle;
    
    #endregion

    #region MonoBehaviour Methods

    private void OnEnable()
    {
        SwapModeManager.SwapModeChanged += SetSwapMode;
    }

    private void OnDisable()
    {
        SwapModeManager.SwapModeChanged -= SetSwapMode;
    }

    private void Awake()
    {
        _dimButton.onClick.AddListener(delegate { ToggleDim(); });
        _cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        _repeaterToggle.onValueChanged.AddListener(delegate { SetRepeater(_repeaterToggle.isOn); });
        _serialControlToggle.onValueChanged.AddListener(delegate { SetSerialControl(_serialControlToggle.isOn); });
        _resetYawButton.onClick.AddListener(delegate { RecenterPose(); });

        _exposureSlider.onValueChanged.AddListener(delegate(float value)
        {
            ExposureValueChanged((int) value);
            PlayerPrefs.SetInt("exposure", (int) value);
            _exposureText.text = "Exposure : " + value;
        });
        
        _rotateCameraButton.onClick.AddListener(delegate { RotateCamera(); });
        _debugUIButton.onClick.AddListener(delegate { DebugMenuPressed(); });
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

    
    public void SetSwapMode(SwapModes mode) 
    {
        //enable/disable serial control toggle depending on if we're using a curtain
        _serialControlToggle.gameObject.SetActive(mode != SwapModes.MANUAL_SWAP); 
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
