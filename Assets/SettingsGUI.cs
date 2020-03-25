using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO.Ports;
using extOSC;

public class SettingsGUI : MonoBehaviour
{
    #region Public field

    public static SettingsGUI instance;

    #endregion

    #region Private Fields

    [SerializeField] private Dropdown _swapModeDropdown;
    [SerializeField] private GameObject _panel;
    [SerializeField] private Slider _pitchSlider, _yawSlider, _rollSlider, _zoomSlider;
    [SerializeField] private Dropdown _serialDropdown, _cameraDropdown;
    [SerializeField] private Text _FPSText;
    [SerializeField] private InputField _IPInputField;
    [SerializeField] private Button _dimButton;
    [SerializeField] private Button _headTrackingOnButton;
    [SerializeField] private Toggle _repeaterToggle;
    [SerializeField] private Text _controlsText;
    [SerializeField] private Text _languageText;
    [SerializeField] private GameObject _mainCamera;

    private bool _monitorGuiEnabled, _oculusGuiEnabled;
    private float _deltaTime = 0.0f;
 
    [SerializeField] private bool serialDebug;

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
        _zoomSlider.onValueChanged.AddListener(delegate { VideoFeed.instance.SetZoom(_zoomSlider.value); });
        
        _serialDropdown.onValueChanged.AddListener(delegate { SelectSerialOption(_serialDropdown.value); });
        _headTrackingOnButton.onClick.AddListener(delegate { VideoFeed.instance.SwitchHeadtracking(); });

        //Assign swap mode dropdown handler
        _swapModeDropdown.onValueChanged.AddListener(delegate { SwapModeManager.instance.SetSwapMode( (SwapModeManager.SwapModes) _swapModeDropdown.value); });
    }

    // Use this for initialization
    private void Start()
    {        
        _monitorGuiEnabled = true;

        SetCameraDropdownOptions();
        SetSwapModeDropdownOptions();
        
        _zoomSlider.value = PlayerPrefs.GetFloat("zoom", 39.5f);

        if (PlayerPrefs.GetInt("repeater") == 1) _repeaterToggle.isOn = true;
        else                                     _repeaterToggle.isOn = false;

        OSCUtilities.GetLocalHost();

        SetLanguageText(PlayerPrefs.GetInt("language"));
    }

    private void Update()
    {     
        if (Input.GetKeyDown("m")) SetMonitorGuiEnabled();

        if (Input.GetKeyDown("z")) SetLanguage(0);
        if (Input.GetKeyDown("x")) SetLanguage(1);
        if (Input.GetKeyDown("c")) SetLanguage(2);

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

        _cameraDropdown.RefreshShownValue();

        ShowFPS();
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

    public void SetMonitorGuiEnabled()
    {
        _monitorGuiEnabled = !_monitorGuiEnabled;

       if (_monitorGuiEnabled)
            _panel.GetComponent<CanvasGroup>().alpha = 0f;
       else
            _panel.GetComponent<CanvasGroup>().alpha = 1f;
    }
                
    public void SetSwapMode(bool useCurtain = false) 
    {
        //show serial dropdown depending on if we're using the curtain or not
        EnableSerialDropdown(useCurtain);

        //show two way swap related networking GUI
        _repeaterToggle.gameObject.SetActive(true);
        _IPInputField.gameObject.SetActive(true);
        SetIpInputField(); //else initialize IP Input field
    }

    public void SetServoMode()
    {
        EnableSerialDropdown(true);

        //hide two way swap related networking GUI
        _IPInputField.gameObject.SetActive(false);
        _repeaterToggle.gameObject.SetActive(false);
    }

    #endregion

    #region Private Methods

    private void SelectSerialOption(int index)
    {
        //if found, set the port by options index
        if (index != -1)
        {
            ArduinoControl.instance.Open(index);
            PlayerPrefs.SetString("Serial Port", _serialDropdown.options[index].text);
        } //TODO notify there was an error if port = -1
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
    
    private void EnableSerialDropdown(bool enable) //shows/hides serial dropdown
    {        
        _serialDropdown.gameObject.SetActive(enable);
        if (enable)
        {
            SetSerialPortDropdownOptions();
            _serialDropdown.RefreshShownValue();
        }
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

    private void SetSerialPortDropdownOptions() //get available ports and add them as options to the dropdown
    {
        string[] ports = SerialPort.GetPortNames();
        _serialDropdown.options.Clear();
        foreach (string c in ports)
        {
            _serialDropdown.options.Add(new Dropdown.OptionData() { text = c });
        }
        
        //TODO only if it is in available options
        var name = PlayerPrefs.GetString("Serial Port");
        _serialDropdown.value = GetSerialIndexByOptionName(_serialDropdown, name); //assign the value that was saved in PlayerPrefs
        ArduinoControl.instance.Open(_serialDropdown.value);
        SelectSerialOption(_serialDropdown.value);
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
