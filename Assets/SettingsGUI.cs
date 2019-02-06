﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;

public class SettingsGUI : MonoBehaviour
{
    #region Private Fields

    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private Slider _pitchSlider, _yawSlider, _rollSlider, _zoomSlider;
    [SerializeField]
    private Dropdown _serialDropdown, _cameraDropdown;
    [SerializeField]
    private Text _FPSText;
    [SerializeField]
    private InputField _IPInputField;
    [SerializeField]
    private Button _dimButton;
    [SerializeField]
    private Toggle _repeaterToggle;

    private VideoFeed _videoFeed;
    private GameObject _mainCamera;
    private OscManager _oscManager;

    private bool _twoWaySwap = true;
    private bool _monitorGuiEnabled, _oculusGuiEnabled;
    private float _deltaTime = 0.0f;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        _videoFeed = FindObjectOfType<VideoFeed>();
        _mainCamera = GameObject.Find("Main Camera");
        _oscManager = FindObjectOfType<OscManager>();

        _cameraDropdown.onValueChanged.AddListener(delegate
        {
            _videoFeed.cameraID = _cameraDropdown.value;
        });

        _dimButton.onClick.AddListener(delegate
        {
            _videoFeed.SetDimmed();
        });

        _IPInputField.onEndEdit.AddListener(delegate { _oscManager.othersIP = _IPInputField.text; });

        _repeaterToggle.onValueChanged.AddListener(delegate { _oscManager.repeater = _repeaterToggle.isOn; });
    }

    // Use this for initialization
    void Start()
    {        
        if (!_twoWaySwap) SetSerialPortDropdownOptions(); //Initialize serial port dropdown if in servo mode
        else SetIpInputField(); //else initialize IP Input field

        _monitorGuiEnabled = true;
        SetCameraDropdownOptions();

        if (PlayerPrefs.GetInt("repeater") == 1) _repeaterToggle.isOn = true;
        else                                     _repeaterToggle.isOn = false;
    }

    void Update()
    {     
        if (Input.GetKeyDown("m")) SetMonitorGuiEnabled();

        if (_videoFeed.useHeadTracking)
        {
            Vector3 pitchYawRoll = _mainCamera.transform.rotation.eulerAngles;
            _rollSlider.value = pitchYawRoll.x;
            _yawSlider.value = 90 - pitchYawRoll.y;
            _pitchSlider.value = pitchYawRoll.z + 90;
            _zoomSlider.value = _videoFeed.zoom;
        }

        _cameraDropdown.RefreshShownValue();

        ShowFPS();
    }

    #endregion

    #region Public Methods

    public void SetMonitorGuiEnabled()
    {
        _monitorGuiEnabled = !_monitorGuiEnabled;

       if (_monitorGuiEnabled)
            _panel.GetComponent<CanvasGroup>().alpha = 0f;
       else
            _panel.GetComponent<CanvasGroup>().alpha = 1f;
    }

    #endregion

    #region Private Methods

    private void SetSerialPortDropdownOptions()
    {
        if (!_twoWaySwap)
        {
            string[] ports = SerialPort.GetPortNames();
            _serialDropdown.options.Clear();
            foreach (string c in ports)
            {
                _serialDropdown.options.Add(new Dropdown.OptionData() { text = c });
            }
            _serialDropdown.value = PlayerPrefs.GetInt("Serial port");
        }
    }

    private void SetIpInputField()
    {
        if (_IPInputField.text != null) _IPInputField.text = PlayerPrefs.GetString("othersIP");
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

    #endregion

}
