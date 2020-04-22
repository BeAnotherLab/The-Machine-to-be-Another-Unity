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

    [SerializeField] private bool _servosOn;
    [SerializeField] private bool _curtainOn;

    public float pitchOffset, yawOffset; //use those values to compensate
    
    #endregion

    #region Private Fields

    [SerializeField] private int _timeOut;
    [SerializeField] private bool _commandOK;

    private Coroutine _timeoutCoroutine;
    private Coroutine _waitForSysReadyCoroutine;
        
    #endregion
    
    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #endregion


    #region Public Methods   

    public void InitialPositions()
    {
        SendCommand("wal_off");
        SendCommand("mir_off");
        SendCommand("cur_off");
    }

    public void ActivateSerial(bool servosOn, bool curtainOn = false)
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;
        if (servosOn) UduinoManager.Instance.BaudRate = 57600;
        else if (_curtainOn) UduinoManager.Instance.BaudRate = 9600;
        _servosOn = servosOn;
        _curtainOn = curtainOn;    
    }

    public void DisableSerial()
    {
        if (_servosOn || _curtainOn) Close();
        _servosOn = false;
        _curtainOn = false;
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
        if (_curtainOn)
        {
            Debug.Log("sending " + command + " to arduino");
            WriteToArduino(command);
        }
    }
    
    public void Close()
    {
       
    }

    #endregion


    #region Private Methods
    
    private void DataReceived(string data, UduinoDevice board)
    {
        Debug.Log("received : " + data);

        if (data == "sys_rdy")
        {
            if(_waitForSysReadyCoroutine != null) StopCoroutine(_waitForSysReadyCoroutine);
            StatusManager.instance.SerialReady();
        } 
        else if (data == "cmd_ok") _commandOK = true;
        else if (data == "TIMEOUT" || data == "MD_FAULT" || data == "MD_BLOCK")
        {
            Debug.Log("ERROR : " + data);
            StatusManager.instance.SerialFailure();
        }
        else if (data == "lng_de") LanguageTextDictionary.instance.LanguageChanged("deutsch");
        else if (data == "lng_fr") LanguageTextDictionary.instance.LanguageChanged("french");
        else if (data == "lng_it") LanguageTextDictionary.instance.LanguageChanged("italian");
        else if (data == "lng_en") LanguageTextDictionary.instance.LanguageChanged("english");
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
    
    #endregion
}