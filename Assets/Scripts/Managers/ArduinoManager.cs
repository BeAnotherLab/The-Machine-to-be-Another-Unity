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
    
    public float pitchOffset, yawOffset; //use those values to compensate
    
    #endregion

    
    #region Private Fields

    [SerializeField] private int _timeOut;

    [SerializeField] private bool _curtainOffOnStandby = true;

    private bool _servosOn; //for one way swap.
    //private bool _commandOK;
    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors
    private bool _sysready; //whether board has been connected already
    //private Coroutine _timeoutCoroutine;
    
    #endregion
    
    
    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        _serialControlOn = PlayerPrefs.GetInt("serialControlOn", 0) == 1;
    }

    private void Start()
    {
        ActivateSerial(false, true);
    }

    #endregion


    #region Public Methods   

    public void InitialPositions()
    {
        if (_curtainOffOnStandby) StartCoroutine(DelayedInitialPositions());
    }

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
            UduinoManager.Instance.BaudRate = 115200; //this is the baudrate for 
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
            //_commandOK = false;
            WriteToArduino(command);
        }
    }
    
    public void Close()
    {
       
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

    private IEnumerator DelayedInitialPositions()
    {
        yield return new WaitForSeconds(3);
        WallOn(false);
        //SendCommand("mir_off");
    }
    
    private void DataReceived(string data, UduinoDevice board)
    {
        Debug.Log("received : " + data, DLogType.System);
        
        //if (data == "cmd_ok") _commandOK = true;
        if (data == "MD_FAULT" || data == "MD_BLOCK")
        {
            Debug.Log("ERROR : " + data, DLogType.Error);
            StatusManager.instance.SerialFailure();
        }
        else if (data == "TIMEOUT")
            Debug.Log("ERROR : " + data, DLogType.Error);
        else if (data == "sysReady")
        {
            Debug.Log("homing done, ready to start");            
        }
    }    
    
    private void WriteToArduino(string message) //send a command, trigger timeout routine
    {
        UduinoManager.Instance.sendCommand(message); 
        //if (_timeoutCoroutine != null) StopCoroutine(_timeoutCoroutine); 
        //_timeoutCoroutine = StartCoroutine(WaitForTimeout());
    }
/*
    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(_timeOut);
        if(!_commandOK) StatusManager.instance.SerialFailure();
    }
/*/
    private IEnumerator WaitForSerial()
    {
        yield return new WaitForSeconds(5f);
        StatusManager.instance.SerialFailure();
    }    
    
    #endregion
}