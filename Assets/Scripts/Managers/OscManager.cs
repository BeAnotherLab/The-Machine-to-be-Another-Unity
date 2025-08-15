using UnityEngine;
using extOSC;
using Mirror.Examples.Pong;
using ScriptableObjectArchitecture; 
using Debug = DebugFile;

public class OscManager : MonoBehaviour {

    #region Public Fields

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
    
    private bool _repeater;
    private bool _connectionEstablished;
    private bool _serialReady;
    
    private OSCReceiver _oscReceiver;
    private OSCTransmitter _oscTransmitter;
    
    #endregion

    #region MonoBehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialReady += CheckConnectionAndSendSerialReady;
        ArduinoManager.SerialFailure += SendSerialFailure;
        UserStateManager.SendThisUserStatus += SendThisUserStatus;
        SettingsGUI.SetRepeater += SetRepeater;
        CustomNetworkManager.ConnectionEstablished += ConnectionEstablished;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialReady -= CheckConnectionAndSendSerialReady;
        ArduinoManager.SerialFailure -= SendSerialFailure;
        UserStateManager.SendThisUserStatus -= SendThisUserStatus;
        SettingsGUI.SetRepeater -= SetRepeater;
        CustomNetworkManager.ConnectionEstablished -= ConnectionEstablished;
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
        _connectionEstablished = false;
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
    private void CheckConnectionAndSendSerialReady()
    {
        _serialReady = true;
        if (_connectionEstablished)
        {
            Debug.Log("sending serial status", DLogType.Network);
            OSCMessage message = new OSCMessage("/serialStatus");
            message.AddValue(OSCValue.Int(1));
            _oscTransmitter.Send(message);    
        }
    }
    
    private void SendSerialFailure()
    {
        Debug.Log("sending serial status", DLogType.Network);
        OSCMessage message = new OSCMessage("/serialStatus");
        message.AddValue(OSCValue.Int(0));
        _oscTransmitter.Send(message);
    }

    private void ConnectionEstablished()
    {
        _connectionEstablished = true;
        if (_serialReady) CheckConnectionAndSendSerialReady();
    }
    
    #endregion

}