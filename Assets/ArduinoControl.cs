/* based on ArduinoConnector by Alan Zucconi
 * http://www.alanzucconi.com/?p=2979
 */
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class ArduinoControl : MonoBehaviour
{

    #region Public Fields

    public static ArduinoControl instance;

    public bool _servosOn;
    public string manualPort;

    public bool useRandomTargets, delay;
    public bool inverse;
    public bool horizontalOnly;
    public float time;

    public List<RandomLerpOnAFloat> _lerpList = new List<RandomLerpOnAFloat>(4);

    /* The baudrate of the serial port. */
    [Tooltip("The baudrate of the serial port")]
    public int baudrate = 57600;

    public float pitchOffset, yawOffset; //use those values to compensate

    #endregion


    #region Private Fields

    private SerialPort _stream;
    private int _serialPort;

    private List<float> pitchBuffer = new List<float>();
    private List<float> yawBuffer = new List<float>();
    private float delayedPitch, delayedYaw;
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _serialPort = PlayerPrefs.GetInt("Serial port");

        if (useRandomTargets)
            for (int i = 0; i < _lerpList.Count; i++)
                _lerpList[i] = gameObject.AddComponent<RandomLerpOnAFloat>();

        _lerpList[0].minimum = 45;
        //_lerpList[1].minimum = 45;

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

            if(inverse && !horizontalOnly)
                sum = 180 - sum;
            if (useRandomTargets)
                sum = _lerpList[0].ChangingValue();
            if (delay) { 
                StartCoroutine(DelayPitchValue(sum, time));
                sum = delayedPitch;
            }

            //sum = 15;
            //Debug.Log("pitch " + sum);
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

            if (inverse)
                sum = 180 - sum;
            if (useRandomTargets)
                sum = _lerpList[1].ChangingValue();
            if (delay)
            {
                StartCoroutine(DelayYawValue(sum, time));
                sum = delayedYaw;
            }
            //sum = 15;
            //Debug.Log("yaw " + sum);
            WriteToArduino("Yaw " + sum);
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

    private IEnumerator DelayPitchValue(float value, float time)    {
        pitchBuffer.Add(value);
        yield return new WaitForSecondsRealtime(time);
        delayedPitch = pitchBuffer[0];
        pitchBuffer.RemoveAt(0);
        Resources.UnloadUnusedAssets();
    }

    private IEnumerator DelayYawValue(float value, float time)
    {
        yawBuffer.Add(value);
        yield return new WaitForSecondsRealtime(time);
        delayedYaw = yawBuffer[0];
        yawBuffer.RemoveAt(0);
        Resources.UnloadUnusedAssets();
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
            if (manualPort != "")
            {
                manualPort = manualPort.Remove(0, 3);

                int portNumber = int.Parse(manualPort);

                if (portNumber < 10)
                    manualPort = "COM" + portNumber;
                else
                    manualPort = "\\\\.\\" + "COM" + portNumber;

                _stream = new SerialPort(manualPort, baudrate);//(port, baudrate)
            }
            else
                _stream = new SerialPort(port, baudrate);
            //Debug.Log(port);
            _stream.Open();
        }
    }

    #endregion
}