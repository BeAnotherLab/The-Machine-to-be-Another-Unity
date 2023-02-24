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
    [SerializeField] private GameObject _panel;
    [SerializeField] private IPInputField _ipInputField;
    [SerializeField] private Toggle _serialControlToggle;
    
    [SerializeField] private Button _cameraSettingsButton;

    [SerializeField] private Button _rotateCameraButton;
    [SerializeField] private Slider _exposureSlider;
    [SerializeField] private Text _exposureText;
    [SerializeField] private Toggle _repeaterToggle;
    //[SerializeField] private Text _controlsText;
    
    private bool _oculusGuiEnabled;
    private float _deltaTime = 0.0f;
 
    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _cameraSettingsButton.onClick.AddListener(delegate { VideoCameraManager.instance.ShowCameraConfigWindow(); });
        
        _repeaterToggle.onValueChanged.AddListener(delegate { CustomOscManager.instance.SetRepeater(_repeaterToggle.isOn); });

        _serialControlToggle.onValueChanged.AddListener(delegate { ArduinoManager.instance.SetSerialControlComputer(_serialControlToggle.isOn); });
        
        //_controlsText.text = _controlsText.text + "\n \nlocal IP adress : " + OSCUtilities.GetLocalHost();

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

        if (PlayerPrefs.GetInt("repeater") == 1) 
            _repeaterToggle.isOn = true;
        else                                    
            _repeaterToggle.isOn = false;

        if (PlayerPrefs.GetInt("serialControlOn") == 1) _serialControlToggle.isOn = true;
        else _serialControlToggle.isOn = false;             
        
        if (PlayerPrefs.GetInt("exposure", 1) != 1)
        {
            _exposureSlider.value = PlayerPrefs.GetInt("exposure");
            _exposureText.text = "Exposure : " + _exposureSlider.value;
        }
        
        OSCUtilities.GetLocalHost();

        SetLanguageText(PlayerPrefs.GetInt("language"));
        
        SetSwapMode(true); //hide serial port dropdown, show repeater toggle, show IP input field
    }

    private void Update()
    {     
        //TODO move out of settings GUI
        /*if (Input.GetKeyDown("m")) ToggleDisplay();

        if (Input.GetKeyDown("f"))
        {
            VideoFeed.instance.FlipHorizontal();
        }*/
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
