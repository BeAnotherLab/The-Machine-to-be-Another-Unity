/* based on ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;
using Uduino;

using Debug = DebugFile;

public class ArduinoManager : MonoBehaviour
{
    #region Public Fields

    public static ArduinoManager instance;
    
    #endregion

    
    #region Private Fields

    [SerializeField] private int _timeOut;

    [SerializeField] private bool _curtainOffOnStandby = true;

    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors
    private bool _sysready; //whether board has been connected already
    
    #endregion
    
    
    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
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
    
    public void ActivateSerial(bool servosOn, bool useCurtain)
    {
        if (servosOn) UduinoManager.Instance.BaudRate = 57600;
        else if (_serialControlOn && useCurtain){ //if we are in Technorama and this computer is connected to the Arduino
            UduinoManager.Instance.OnDataReceived += DataReceived;
            UduinoManager.Instance.BaudRate = 115200; //this is the baudrate for //TODO ???
            //TODO what's the baudrate for the Technorama setup?
        }
    }

    public void DisableSerial()
    {
        _serialControlOn = false;
    }

    public void WallOn(bool on)
    {
        if (on) SendCommand("wallOn" );
        else if (!on) SendCommand("wallOff");
    }
    
    public void SendCommand(string command) //used to send commands to control technorama walls, curtains, etc
    {
        if (_serialControlOn)
        {
            Debug.Log("sending " + command + " to arduino");
            WriteToArduino(command);
        }
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
    
    private void DataReceived(string data, UduinoDevice board)
    {
        Debug.Log("received : " + data, DLogType.System);
        
        if (data == "MD_FAULT" || data == "MD_BLOCK")
        {
            Debug.Log("ERROR : " + data, DLogType.Error);
            StatusManager.instance.SerialFailure();
        }
        else if (data == "TIMEOUT") Debug.Log("ERROR : " + data, DLogType.Error);
        else if (data == "sysReady") Debug.Log("homing done, ready to start");            
    }    
    
    private void WriteToArduino(string message) //send a command, trigger timeout routine
    {
        UduinoManager.Instance.sendCommand(message); 
    }

    #endregion
}