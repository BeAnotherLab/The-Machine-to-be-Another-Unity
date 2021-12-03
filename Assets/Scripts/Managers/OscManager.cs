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

    public static OscManager instance;

    public string othersIP { get { return othersIP; } set { SetOthersIP(value); } }   
    
    public delegate void OtherStatus();
    public static OtherStatus OnOtherStatus;
    
    #endregion

    #region Private Fields

    [SerializeField] private UserStatesGameEvent _userStatesGameEvent;
    [SerializeField] private UserStatesVariable _userStates;
    private Camera _mainCamera;

    private OSCTransmitter _oscTransmitter;
    private OSCReceiver _oscReceiver;
    
    private bool _repeater;
    private bool _serialStatusOKReceived;
    private bool _sendHeadTracking;
    
    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        
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
        _oscReceiver.Bind("/serialStatus", ReceiveSerialStatus);
        _oscReceiver.Bind("/serialConfirmation", ReceiveSerialStatusOK);
        _oscReceiver.Bind("/language", ReceiveLanguageChange);
        _oscReceiver.Bind("/startThreatTask", ReceiveThreatStart);
        for (int i = 0; i < 11; i++) _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

        //set IP address of other 
        SetOthersIP(PlayerPrefs.GetString("othersIP"));
        
    }   
    
    #endregion

    #region Public Methods

    public void SendThreatTaskStart(ThreatOrder target)
    {
        OSCMessage message = new OSCMessage("/startThreatTask");
        message.AddValue(OSCValue.String(target.ToString()));
        _oscTransmitter.Send(message);
    }
    
    public void SendLanguageChange(string language)
    {
        OSCMessage message = new OSCMessage("/language");
        message.AddValue(OSCValue.String(language));
        _oscTransmitter.Send(message);
        Debug.Log("send language change message", DLogType.Network);
    }
    
    public void SetSendHeadtracking(bool send)
    {
        _sendHeadTracking = true;
    }
    
    public void EnableRepeater(bool enable) 
    {
        if (enable)
        {
            //set repeater based on what was stored in playerprefs
            if (PlayerPrefs.GetInt("repeater") == 0) SetRepeater(false);
            else SetRepeater(true);
        }
        else _repeater = false;
    }

    public void SetRepeater(bool r)
    {
        _repeater = r;
        if (r) PlayerPrefs.SetInt("repeater", 1);
        else PlayerPrefs.SetInt("repeater", 0);
    }

    public void SendThisUserStatus(UserStatus status)
    {
        OSCMessage message = new OSCMessage("/otherUser");

        int i = 0;
        
        if (status == UserStatus.headsetOff) i = 0;
        else if (status == UserStatus.headsetOn) i = 1;
        else if (status == UserStatus.readyToStart) i = 2;
        
        message.AddValue(OSCValue.Int(i));
        _oscTransmitter.Send(message);
        Debug.Log("sending user status : " + status, DLogType.Network);
    }

    public void SendSerialStatus(bool status)
    {
        if(status) StartCoroutine(SendStatusUntilAnswer()); //when sending OK, we must wait for answer
        else if (_repeater) //send not OK
        {
            OSCMessage message = new OSCMessage("/serialStatus");
            message.AddValue(OSCValue.Int(0));
            _oscTransmitter.Send(message);
        }
        Debug.Log("sending serial status : " + status, DLogType.Network);
    }

    #endregion

    #region Private Methods

    private void ReceiveThreatStart(OSCMessage message)
    {
        string value;
        if (message.ToString(out value))
            ThreatManager.instance.StartTask((ThreatOrder)Enum.Parse(typeof(ThreatOrder), value));            
    }
    
    private void SetOthersIP(string othersIP)
    {
        PlayerPrefs.SetString("othersIP", othersIP);
        GetComponent<OSCTransmitter>().RemoteHost = othersIP;
    }
   
    private void ReceiveLanguageChange(OSCMessage message)
    {
        string value;
        if(message.ToString(out value))
            LocalizationManager.instance.LoadLocalizedText(value);
        Debug.Log("received language change : " + value, DLogType.Network);
    }
    
    private void ReceiveCalibrate(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) VideoFeed.instance.RecenterPose();

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) VideoFeed.instance.Dim(true);

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOff(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) VideoFeed.instance.Dim(false);

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveBtn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
        {
            if (value == 1f) {
                for (int i = 0; i < 11; i++)
                    if (message.Address == "/btn" + i.ToString()) AudioManager.instance.GetComponent<AudioManager>().PlaySound(i);
            }
        }
        
        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceivedOtherStatus(OSCMessage message)
    {
        if (StatusManager.instance.presenceDetection)
        {
            int x;
            if (message.ToInt(out x))
            {
                //_userStates.SetValue()
                //_userStatesGameEvent.Raise(_userStates);
                if (x == 0) _userStates.Value.otherStatus = UserStatus.headsetOff; //StatusManager.instance.OtherLeft();
                else if (x == 1) _userStates.Value.otherStatus = UserStatus.headsetOn; //StatusManager.instance.OtherPutHeadsetOn();
                if (x == 2) _userStates.Value.otherStatus = UserStatus.readyToStart; //StatusManager.instance.OtherUserIsReady();
            }

            try { OnOtherStatus(); } //when receiving other status over OSC we get an error?
            catch (Exception e) { }
        }
    }

    private void ReceiveSerialStatus(OSCMessage message)
    {
        if (StatusManager.instance.presenceDetection)
        {
            int x;
            if (message.ToInt(out x))
            {
                if (x == 0) StatusManager.instance.SerialFailure();
                else if (x == 1) //when we receive serial ready from computer connected to Arduino
                {
                    //confirm we've received the message
                    OSCMessage oscMessage = new OSCMessage("/serialConfirmation");
                    _oscTransmitter.Send(oscMessage);
                    StatusManager.instance.SerialReady();
                }
            }
            Debug.Log("received serial confirmation", DLogType.Network);
        }
    }

    private void ReceiveSerialStatusOK(OSCMessage message) //the receiver computer acknowledges serial status OK
    {
        _serialStatusOKReceived = true;
        StatusManager.instance.SerialReady(true);
        Debug.Log("acknowledge serial status OK", DLogType.Network);
    }
    
    private IEnumerator SendStatusUntilAnswer()
    {
        Debug.Log("sending serial status", DLogType.Network);
        OSCMessage message = new OSCMessage("/serialStatus");
        message.AddValue(OSCValue.Int(1));
        _oscTransmitter.Send(message);
        yield return new WaitForSeconds(1);

        if (!_serialStatusOKReceived) StartCoroutine(SendStatusUntilAnswer());
    }
    
    #endregion

}