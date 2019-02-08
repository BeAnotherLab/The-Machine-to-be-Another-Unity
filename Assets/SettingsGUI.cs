using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;
using extOSC;

public class SettingsGUI : MonoBehaviour
{
    #region Public field

    public SettingsGUI instance;

    #endregion  

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
    private Button _headTrackingOnButton;
    [SerializeField]
    private Toggle _repeaterToggle;
    [SerializeField]
    private Text _controlsText;
    [SerializeField]
    private Toggle _autoSwapToggle, _manualSwapToggle, _servoSwapToggle;

    [SerializeField]
    private GameObject _mainCamera;

    private bool _twoWaySwap = true;
    private bool _monitorGuiEnabled, _oculusGuiEnabled;
    private float _deltaTime = 0.0f;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        //objects in the scene
        _mainCamera = GameObject.Find("Main Camera");

        _cameraDropdown.onValueChanged.AddListener(delegate
        {
            VideoFeed.instance.cameraID = _cameraDropdown.value;
        });

        _dimButton.onClick.AddListener(delegate
        {
            VideoFeed.instance.SetDimmed();
        });

        _IPInputField.onEndEdit.AddListener(delegate { OscManager.instance.othersIP = _IPInputField.text; });

        _repeaterToggle.onValueChanged.AddListener(delegate { OscManager.instance.SetRepeater(_repeaterToggle.isOn); });

        _controlsText.text = _controlsText.text + "\n \nlocal IP adress : " + OSCUtilities.GetLocalHost();

        //Assign servos control buttons handlers
        _pitchSlider.onValueChanged.AddListener(delegate { ArduinoControl.instance.SetPitch(_pitchSlider.value); });
        _yawSlider.onValueChanged.AddListener(delegate { ArduinoControl.instance.SetYaw(_yawSlider.value); });
        _serialDropdown.onValueChanged.AddListener(delegate { ArduinoControl.instance.SetSerialPort(_serialDropdown.value); });
        _headTrackingOnButton.onClick.AddListener(delegate { VideoFeed.instance.SwitchHeadtracking(); });

        //Assign swap mode toggles handlers
        _autoSwapToggle.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode(SwapModeManager.SwapModes.AUTO_SWAP); });
        _manualSwapToggle.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode(SwapModeManager.SwapModes.MANUAL_SWAP); });
        _servoSwapToggle.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode(SwapModeManager.SwapModes.SERVO_SWAP); });

    }

    // Use this for initialization
    private void Start()
    {        
        if (!_twoWaySwap) SetSerialPortDropdownOptions(); //Initialize serial port dropdown if in servo mode
        else SetIpInputField(); //else initialize IP Input field

        _monitorGuiEnabled = true;

        SetCameraDropdownOptions();

        if (PlayerPrefs.GetInt("repeater") == 1) _repeaterToggle.isOn = true;
        else                                     _repeaterToggle.isOn = false;

        OSCUtilities.GetLocalHost();
    }

    private void Update()
    {     
        if (Input.GetKeyDown("m")) SetMonitorGuiEnabled();

        if (VideoFeed.instance.useHeadTracking)
        {
            Vector3 pitchYawRoll = utilities.toEulerAngles(_mainCamera.transform.rotation);

            _rollSlider.value = pitchYawRoll.x;
            _yawSlider.value = 90 - pitchYawRoll.y;
            _pitchSlider.value = pitchYawRoll.z + 90;
            _zoomSlider.value = VideoFeed.instance.zoom;
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
