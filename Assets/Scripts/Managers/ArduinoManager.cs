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

    private bool _servosOn; //for one way swap.
    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors
    
    private bool _invertX, _invertY;

    #endregion
    
    
    #region MonoBehaviour Methods
    
    private void OnEnable()
    {
        InvertServos.InvertAxis += InvertAxis;
    }

    private void OnDisable()
    {
        InvertServos.InvertAxis -= InvertAxis;

    }
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
        if (serialControlOn) PlayerPrefs.SetInt("serialControlOn", 1);
        else PlayerPrefs.SetInt("serialControlOn", 0);    
        _serialControlOn = serialControlOn;
    }
    
    public void ActivateSerial(bool servosOn, bool useCurtain)
    {
        if (servosOn) UduinoManager.Instance.BaudRate = 115200;
        else if (_serialControlOn && useCurtain){
            UduinoManager.Instance.OnDataReceived += DataReceived;
            UduinoManager.Instance.BaudRate = 9600; //if we are in Technorama and this computer is connected to the Arduino
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
            if (_invertY)
            {
                GetComponent<UduinoManager>().sendCommand("p", (int) 180 - value);
            }
            else
            {
                GetComponent<UduinoManager>().sendCommand("p", (int) value);
            } 
        }
    }

    public void SetYaw(float value)
    {
        if (_servosOn)
        {
            if (_invertX)
            {
                GetComponent<UduinoManager>().sendCommand("y", (int) 180 - value);
            }
            else
            {
                GetComponent<UduinoManager>().sendCommand("y", (int) value);
            }
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
        SendCommand("init");
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
            OscManager.instance.SendSerialStatus(true);
            Debug.Log("homing done, ready to start");            
        }
    }    
    
    private void WriteToArduino(string message) //send a command, trigger timeout routine
    {
        UduinoManager.Instance.sendCommand(message); 
    }

    private IEnumerator WaitForSerial()
    {
        yield return new WaitForSeconds(5f);
        StatusManager.instance.SerialFailure();
    }    
    
    private void InvertAxis(bool x, bool y)
    {
        _invertX = x; _invertY = y;
    }    

    #endregion
}