
using UnityEngine;
using System;
using System.Collections;
using Uduino;

using Debug = DebugFile;

public class ArduinoManager : MonoBehaviour
{
    #region Public Fields

    #endregion

    
    #region Private Fields

    public delegate void OnSerialFailure();
    public static OnSerialFailure SerialFailure;
    
    [SerializeField] private int _timeOut;

    [SerializeField] private bool _curtainOffOnStandby = true;

    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors
    private bool _sysready; //whether board has been connected already
    
    #endregion
    
    
    #region MonoBehaviour Methods

    private void OnEnable()
    {
        SettingsGUI.SetSerialControl += SetSerialControlComputer;
        StatusManager.SendArduinoCommand += SendCommand;
        SerialDebugPanel.SendArduinoCommand += SendCommand;
    }

    private void OnDisable()
    {
        SettingsGUI.SetSerialControl -= SetSerialControlComputer;
        StatusManager.SendArduinoCommand -= SendCommand;
        SerialDebugPanel.SendArduinoCommand -= SendCommand;
    }

    private void Awake()
    {
        _serialControlOn = PlayerPrefs.GetInt("serialControlOn", 0) == 1;
    }
    
    #endregion


    #region Public Methods   

    public void SetSerialControlComputer(bool serialControlOn) //defines if this computer is the one in charge of serial control in Technorama swap
    {
        if (serialControlOn) PlayerPrefs.SetInt("serialControlOn", 1);
        else PlayerPrefs.SetInt("serialControlOn", 0);    
        _serialControlOn = serialControlOn;
    }
    
    public void ActivateSerial(bool servosOn, bool useCurtain) //TODO remove?
    {
        if (servosOn) UduinoManager.Instance.BaudRate = 57600;
        else if (_serialControlOn && useCurtain){ //if we are in Technorama and this computer is connected to the Arduino
            UduinoManager.Instance.OnDataReceived += DataReceived;
            UduinoManager.Instance.BaudRate = 115200; //this is the baudrate for //TODO ???
            //TODO what's the baudrate for the Technorama setup?
        }
    }

    public void WallOn(bool on)
    {
        if (on) SendCommand("wal_on" );
        else if (!on) SendCommand("wal_off");
    }
    
    public void ArduinoBoardConnected()
    {
        Debug.Log("board connected");
        if (!_sysready)
        {
            _sysready = true;
            SendCommand("init");
        }
    }
    
    #endregion


    #region Private Methods
    
    private void SendCommand(string command) //used to send commands to control technorama walls, curtains, etc
    {
        if (_serialControlOn)
        {
            Debug.Log("sending " + command + " to arduino");
            UduinoManager.Instance.sendCommand(command); 
        }
    }
    
    private void DataReceived(string data, UduinoDevice board)
    {
        Debug.Log("received : " + data, DLogType.System);
        
        if (data == "MD_FAULT" || data == "MD_BLOCK")
        {
            Debug.Log("ERROR : " + data, DLogType.Error);
            SerialFailure();
        }
        else if (data == "TIMEOUT") Debug.Log("ERROR : " + data, DLogType.Error);
        else if (data == "sysReady") Debug.Log("homing done, ready to start");            
    }    

    #endregion
}