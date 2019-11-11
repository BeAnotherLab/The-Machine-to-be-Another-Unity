/* based on ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO.Ports;

public class ArduinoControl : MonoBehaviour
{

    #region Public Fields

    public static ArduinoControl instance;

    public bool _servosOn;

    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 57600;

    public float pitchOffset, yawOffset; //use those values to compensate

    #endregion


    #region Private Fields

    private SerialPort _stream;
    private int _serialPort;

    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _serialPort = PlayerPrefs.GetInt("Serial port");
    }

    #endregion


    #region Public Methods   

    public void ActivateServos(bool activate)
    {
        if (_servosOn) Close();
        if (activate) Open(_serialPort);
        _servosOn = activate;
    }

    public void SetSerialPort(int p)
    {
        Open(p);
        _serialPort = p;
        PlayerPrefs.SetInt("Serial port", p);
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

    public void SendCurtainMessage(int down) {
        if (_servosOn)
        {
            int onOff;

            if(down == 1) {
                onOff = 1;
                Debug.Log("askin arduino to close curtain");
                WriteToArduino("Curtain_down " + onOff);
            }

            else 
            {
                onOff = 1;
                Debug.Log("askin arduino to close curtain");
                WriteToArduino("Curtain_up " + onOff);
            }
        }
    }

    public string ReadFromArduino(int timeout = 0)
    {
        _stream.ReadTimeout = timeout;
        try
        {
            return _stream.ReadLine();
        }
        catch (TimeoutException)
        {
            return null;
        }
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
                dataString = _stream.ReadLine();
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
        if (_stream != null)
            _stream.Close();
    }

    #endregion


    #region Private Methods

    private void WriteToArduino(string message)
    {
        if (_stream != null)
        {
            // Send the request
            _stream.WriteLine(message);
            _stream.BaseStream.Flush();
        }
    }

    private void Open(int p)
    {
        string[] ports = SerialPort.GetPortNames();
        string port = "";
        if (ports.Length == 1)
            p = 0;
        if (p < ports.Length)
            port = ports[p];
        if (_stream != null)
            _stream.Close();

        if (port != "")
        {
            _stream = new SerialPort(port, baudrate);
            _stream.Open();
        }
    }

    #endregion
}