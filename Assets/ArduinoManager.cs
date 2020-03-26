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

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        UduinoManager.Instance.OnDataReceived += DataReceived;
    }

    #endregion


    #region Public Methods   

    public void ActivateSerial(bool servosOn, bool curtainOn = false)
    {
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
        Debug.Log(data);
    }
    
    private void WriteToArduino(string message)
    {
        UduinoManager.Instance.sendCommand(message);
    }

    #endregion
}