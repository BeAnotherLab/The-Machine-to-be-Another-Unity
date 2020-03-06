/* based on ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using System;
using System.Collections;

public class ArduinoManager : MonoBehaviour
{

    #region Public Fields

    public static ArduinoManager instance;

    [SerializeField] private bool _servosOn;
    [SerializeField] private bool _curtainOn;

    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 57600;

    public float pitchOffset, yawOffset; //use those values to compensate
    
    #endregion


    #region Private Fields


    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #endregion


    #region Public Methods   

    public void ActivateSerial(bool servosOn, bool curtainOn = false)
    {
        if (servosOn) baudrate = 57600;
        else if (_curtainOn) baudrate = 9600;
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

    public string ReadFromArduino(int timeout = 0)
    {
        return "";
    }

    public IEnumerator AsynchronousReadFromArduino(Action<string> callback, Action fail = null, float timeout = float.PositiveInfinity)
    {
        DateTime initialTime = DateTime.Now;
        DateTime nowTime;
        TimeSpan diff = default(TimeSpan);

        string dataString = null;

        do
        {
            // A single read attempt
            try
            {
            }
            catch (TimeoutException)
            {
                dataString = null;
            }

            if (dataString != null)
            {
                callback(dataString);
                yield return null;
            }
            else
                yield return new WaitForSeconds(0.05f);

            nowTime = DateTime.Now;
            diff = nowTime - initialTime;

        } while (diff.Milliseconds < timeout);

        if (fail != null)
            fail();
        yield return null;
    }

    void GetPlayerPrefs()
    {
        pitchOffset = PlayerPrefs.GetFloat("pitchOffset");
        yawOffset = PlayerPrefs.GetFloat("yawOffset");
    }

    public void Close()
    {
       
    }

    #endregion


    #region Private Methods

    private void WriteToArduino(string message)
    {
        
    }

    #endregion
}