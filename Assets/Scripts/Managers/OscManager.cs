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

    public string othersIP { get { return othersIP; } set { SetOthersIP(value); } }   //TODO remove?
    //TODO ???
    public delegate void OtherStatus();
    public static OtherStatus OnOtherStatus;
    
    public delegate void OnReceivedAudioButtonPressed(int i);
    public static OnReceivedAudioButtonPressed ReceivedAudioButtonPressed;

    public delegate void OnReceiveRecenterPose();
    public static OnReceiveRecenterPose ReceiveRecenterPose;

    
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
        StatusManager.SendThisUserStatus += SendThisUserStatus;
        SettingsGUI.SetRepeater += SetRepeater;
    }

    private void OnDisable()
    {
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
        _oscReceiver.Bind("/curtain", ReceiveCurtain); //TODO remove?
        for (int i = 0; i < 11; i++) _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

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
    }

    private void ReceiveDimOff(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) _dimGameEvent.Raise(false);

        if (_repeater) _oscTransmitter.Send(message);
    }

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

    private void ReceiveCurtain(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            _curtainOnGameEvent.Raise(value == 1);
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

    #endregion

}