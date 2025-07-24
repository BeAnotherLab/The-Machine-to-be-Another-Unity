
using UnityEngine;
using System;
using System.Collections;
using Uduino;

using Debug = DebugFile;

public class ArduinoManager : MonoBehaviour
{
  
    #region Private Fields

    public delegate void OnSerialFailure();
    public static OnSerialFailure SerialFailure;
    
    public delegate void OnSerialReady();
    public static OnSerialReady SerialReady;
    
    [SerializeField] private int _timeOut;

    [SerializeField] private bool _curtainOffOnStandby = true;

    private bool _serialControlOn; //for technorama swap. determine if this computer is in charge of controlling the curtain and mirrors
    
    #endregion
    
    
    #region MonoBehaviour Methods

    private void OnEnable()
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;
        SettingsGUI.SetSerialControl += SetSerialControlComputer;
        StatusManager.SendArduinoCommand += SendCommand;
        SerialDebugPanel.SendArduinoCommand += SendCommand; //TODO add this panel or remove
    }

    private void OnDisable()
    {
        UduinoManager.Instance.OnDataReceived -= DataReceived;
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

    public void WallOn(bool on)
    {
        if (on) SendCommand("wal_on" );
        else if (!on) SendCommand("wal_off");
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
        
        //TODO this is only for Technorama
        if (data == "MD_FAULT" || data == "MD_BLOCK" || data == "TIMEOUT")
        {
            Debug.Log("ERROR : " + data, DLogType.Error);
            SerialFailure();
        }
        else if (data == "sys_rdy")
        {
            SerialReady();
            Debug.Log("homing done, ready to start");
        }            
    }    

    #endregion
}