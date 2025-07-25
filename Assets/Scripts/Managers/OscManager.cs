using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using VRStandardAssets.Menu;
using extOSC;
using ScriptableObjectArchitecture;
using Debug = DebugFile;

public class OscManager : MonoBehaviour {

    #region Public Fields

    public delegate void OtherStatus(); //TODO remove?
    public static OtherStatus OnOtherStatus;
    
    public delegate void OnReceivedAudioButtonPressed(int i);
    public static OnReceivedAudioButtonPressed ReceivedAudioButtonPressed;

    public delegate void OnReceiveRecenterPose();
    public static OnReceiveRecenterPose ReceiveRecenterPose;

    public delegate void OnReceiveSerialReady();
    public static OnReceiveSerialReady ReceiveSerialReady;
    
    public delegate void OnReceiveSerialFailure();
    public static OnReceiveSerialFailure ReceiveSerialFailure;
    
    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    public UserStateGameEvent otherStateGameEvent;
    
    #endregion
 
    #region Private Fields

    [SerializeField] private BoolGameEvent _dimGameEvent;
    [SerializeField] private BoolGameEvent _curtainOnGameEvent;

    private OSCReceiver _oscReceiver;
    
    private bool _repeater;
    private bool _serialStatusOKReceived;
    private bool _sendHeadTracking;
    private OSCTransmitter _oscTransmitter;
    
    #endregion

    #region MonoBehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialReady += SendSerialReady;
        ArduinoManager.SerialFailure += SendSerialFailure;
        StatusManager.SendThisUserStatus += SendThisUserStatus;
        SettingsGUI.SetRepeater += SetRepeater;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialReady -= SendSerialReady;
        ArduinoManager.SerialFailure -= SendSerialFailure;
        StatusManager.SendThisUserStatus -= SendThisUserStatus;
        SettingsGUI.SetRepeater -= SetRepeater;
    }

    private void Awake()
    {
        _oscReceiver = GetComponent<OSCReceiver>();
        _oscTransmitter = GetComponent<OSCTransmitter>();
    }

    private void Start()
    {
        //assign handlers to messages
        _oscReceiver.Bind("/otherUser", ReceivedOtherStatus);
        _oscReceiver.Bind("/dimon", ReceiveDimOn);
        _oscReceiver.Bind("/dimoff", ReceiveDimOff);
        _oscReceiver.Bind("/ht", ReceiveCalibrate);
        for (int i = 0; i < 11; i++) _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

        _oscReceiver.Bind("/serialStatus", ReceiveSerialStatus);
        
        //set IP address of other 
        SetOthersIP(PlayerPrefs.GetString("othersIP"));
    }   
    
    #endregion

    #region Public Methods
    
    private void SetRepeater(bool r)
    {
        _repeater = r;
        if (r) PlayerPrefs.SetInt("repeater", 1);
        else PlayerPrefs.SetInt("repeater", 0);
    }

    public void SendThisUserStatus(UserState status)
    {
        OSCMessage message = new OSCMessage("/otherUser");

        int i = 0;
        
        if (status == UserState.headsetOff) i = 0;
        else if (status == UserState.headsetOn) i = 1;
        else if (status == UserState.readyToStart) i = 2;
        
        message.AddValue(OSCValue.Int(i));
        _oscTransmitter.Send(message);
        Debug.Log("sending user status : " + status, DLogType.Network);
    }

    #endregion

    #region Private Methods
    
    private void SetOthersIP(string othersIP)
    {
        PlayerPrefs.SetString("othersIP", othersIP);
        GetComponent<OSCTransmitter>().RemoteHost = othersIP;
    }

    private void ReceiveCalibrate(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) ReceiveRecenterPose();

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) _dimGameEvent.Raise(true);

        if (_repeater) _oscTransmitter.Send(message);
    } //TODO collapse into one dim

    private void ReceiveDimOff(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) _dimGameEvent.Raise(false);

        if (_repeater) _oscTransmitter.Send(message);
    } //TODO collapse into one dim

    private void ReceiveBtn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
        {
            if (value == 1f) {
                for (int i = 0; i < 11; i++)
                    if (message.Address == "/btn" + i) ReceivedAudioButtonPressed(i);
            }
        }
        
        if (_repeater) _oscTransmitter.Send(message);
    }

    public void SendBtn(int index) 
    {
        OSCMessage message = new OSCMessage("/btn" + index.ToString());
        message.AddValue(OSCValue.Float(1));
        _oscTransmitter.Send(message); //            
    }
    
    private void ReceivedOtherStatus(OSCMessage message)
    {
            int x;
            if (message.ToInt(out x))
            {
                previousOtherState.Value = otherState.Value;

                if (x == 0) otherState.Value = UserState.headsetOff; //StatusManager.instance.OtherLeft();
                else if (x == 1) otherState.Value = UserState.headsetOn; //StatusManager.instance.OtherPutHeadsetOn();
                else if (x == 2) otherState.Value = UserState.readyToStart; //StatusManager.instance.OtherUserIsReady();
                
                otherStateGameEvent.Raise(otherState);
            }

            try { OnOtherStatus(); } //when receiving other status over OSC we get an error?
            catch (Exception e) { }
            
    }
    private void ReceiveSerialStatus(OSCMessage message) //this is only for receiving OK to start,
    {
        int x;
        if (message.ToInt(out x))
        {
            if (x == 1) 
            {
                ReceiveSerialReady(); //when we receive serial ready from computer connected to Arduino
                Debug.Log("received serial confirmation", DLogType.Network);
            }
            else if (x == 0) 
            {
                ReceiveSerialFailure(); //when we receive serial error from computer connected to Arduino
                Debug.Log("received serial error message", DLogType.Network);
            }
        }
    }
    private void SendSerialReady()
    {
        Debug.Log("sending serial status", DLogType.Network);
        OSCMessage message = new OSCMessage("/serialStatus");
        message.AddValue(OSCValue.Int(1));
        _oscTransmitter.Send(message);
    }
    
    private void SendSerialFailure()
    {
        Debug.Log("sending serial status", DLogType.Network);
        OSCMessage message = new OSCMessage("/serialStatus");
        message.AddValue(OSCValue.Int(0));
        _oscTransmitter.Send(message);
    }
    
    #endregion

}