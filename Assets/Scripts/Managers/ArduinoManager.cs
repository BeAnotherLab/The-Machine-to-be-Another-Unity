/* based on ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using Uduino;

public class ArduinoManager : MonoBehaviour
{

    #region Public Fields

    public static ArduinoManager instance;

    [SerializeField] private bool _servosOn; //for one way swap.
    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors

    public float pitchOffset, yawOffset; //use those values to compensate
    
    #endregion

    
    #region Private Fields

    [SerializeField] private int _timeOut;
    private bool _commandOK;

    private Coroutine _timeoutCoroutine;
    private Coroutine _waitForSysReadyCoroutine;
        
    #endregion
    
    
    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        _serialControlOn = PlayerPrefs.GetInt("serialControlOn", 0) == 1;
    }
    
    #endregion


    #region Public Methods   

    public void InitialPositions()
    {
        StartCoroutine(DelayedInitialPositions());
    }

    public void SetSerialControlComputer(bool serialControlOn) //defines if this computer is the one in charge of serial control in Technorama swap
    {
        if(serialControlOn) PlayerPrefs.SetInt("serialControlOn", 1);
        else PlayerPrefs.SetInt("serialControlOn", 0);
        _serialControlOn = serialControlOn;
    }
    
    public void ActivateSerial(bool servosOn, bool useCurtain)
    {
        if (servosOn) UduinoManager.Instance.BaudRate = 57600;
        else if (_serialControlOn && useCurtain){
            UduinoManager.Instance.OnDataReceived += DataReceived;
            UduinoManager.Instance.BaudRate = 9600; //if we are in Technorama and this computer is connected to the Arduino
            _waitForSysReadyCoroutine = StartCoroutine(WaitForSerial());
        }
        _servosOn = servosOn;
    }

    public void DisableSerial()
    {
        if (_servosOn || _serialControlOn) Close();
        _servosOn = false;
        _serialControlOn = false;
    }

    public void Open(int p)
    {
     
    }

    public void SetPitch(float value)
    {
        if (_servosOn)
        {
            float sum;
            sum = value + pitchOffset;
            if ((value + pitchOffset) > 180) sum = 179.5f;
            if ((value + pitchOffset) < 0) sum = 0.5f;
            WriteToArduino("Pitch " + sum);
        }
    }

    public void SetYaw(float value)
    {
        if (_servosOn)
        {
            float sum;
            sum = value + yawOffset;
            if ((value + yawOffset) > 180) sum = 179.5f;
            if ((value + yawOffset) < 0) sum = 0.5f;
            WriteToArduino("Yaw " + sum);
        }
    }

    public void SendCommand(string command) //used to send commands to control technorama walls, curtains, etc
    {
        if (_serialControlOn)
        {
            Debug.Log("sending " + command + " to arduino");
            _commandOK = false;
            WriteToArduino(command);
        }
    }
    
    public void Close()
    {
       
    }

    #endregion


    #region Private Methods

    private IEnumerator DelayedInitialPositions()
    {
        yield return new WaitForSeconds(3);
        SendCommand("wal_off");
        SendCommand("mir_off");
        SendCommand("cur_on");
    }
    
    private void DataReceived(string data, UduinoDevice board)
    {
        Debug.Log("received : " + data);

        if (data == "sys_rdy")
        {
            if(_waitForSysReadyCoroutine != null) StopCoroutine(_waitForSysReadyCoroutine);
            OscManager.instance.SendSerialStatus(true);
        } 
        else if (data == "cmd_ok") _commandOK = true;
        else if (data == "TIMEOUT" || data == "MD_FAULT" || data == "MD_BLOCK")
        {
            Debug.Log("ERROR : " + data);
            StatusManager.instance.SerialFailure();
        }
        else if (data == "lng_de") LocalizationManager.instance.LoadLocalizedText("LocalizedText_de.json");
        else if (data == "lng_fr") LocalizationManager.instance.LoadLocalizedText("LocalizedText_fr.json");
        else if (data == "lng_it") LocalizationManager.instance.LoadLocalizedText("LocalizedText_it.json");
        else if (data == "lng_en") LocalizationManager.instance.LoadLocalizedText("LocalizedText_en.json");
    }
    
    private void WriteToArduino(string message) //send a command, trigger timeout routine
    {
        UduinoManager.Instance.sendCommand(message); 
        if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine); 
        _timeoutCoroutine = StartCoroutine(WaitForTimeout());
    }

    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(_timeOut);
        if(!_commandOK) StatusManager.instance.SerialFailure();
    }

    private IEnumerator WaitForSerial()
    {
        yield return new WaitForSeconds(5f);
        StatusManager.instance.SerialFailure();
    }    
    
    #endregion
}