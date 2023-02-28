using UnityEngine;
using extOSC;
using ScriptableObjectArchitecture;
using Debug = DebugFile;

public class CustomOscManager : MonoBehaviour {

    #region Public Fields

    public static CustomOscManager instance;

    public string othersIP { get { return othersIP; } set { SetOthersIP(value); } }   
    
    #endregion

    #region Private Fields
    
    private OSCTransmitter _oscTransmitter;
    private OSCReceiver _oscReceiver;
    
    private bool _repeater;
    private bool _sendHeadTracking;

    [SerializeField] private GameEvent _receivedStartInstructionsGameEvent;
    [SerializeField] private BoolGameEvent _receiveNoVRGameEvent;
    
    #endregion

    #region MonoBehaviour Methods
    
    private void Awake()
    {
        if (instance == null) instance = this;

        _oscReceiver = GetComponent<OSCReceiver>();
        _oscTransmitter = GetComponent<OSCTransmitter>();
    }

    private void Start()
    {
        //assign handlers to messages
        _oscReceiver.Bind("/dimon", ReceiveDimOn);
        _oscReceiver.Bind("/dimoff", ReceiveDimOff);
        _oscReceiver.Bind("/ht", ReceiveCalibrate);
        _oscReceiver.Bind("/language", ReceiveLanguageChange);
        _oscReceiver.Bind("/startInstruction", ReceiveStartInstruction);
        _oscReceiver.Bind("/noVR", ReceiveNoVR);
        for (int i = 0; i < 11; i++) _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

        //set IP address of other 
        SetOthersIP(PlayerPrefs.GetString("othersIP"));
        
        //for george manual hyperscanning swap
        SetSendHeadtracking(true);
        
        _repeater = PlayerPrefs.GetInt("repeater") == 1;
    }

    #endregion

    #region Public Methods
    
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
    
    public void SendStartInstruction()
    {
        if (_repeater)
        {
            OSCMessage message = new OSCMessage("/startInstruction");
            message.AddValue(OSCValue.Int(1));
            _oscTransmitter.Send(message);
            Debug.Log("send start instruction", DLogType.Network);    
        }
    }
    
    public void SendNoVRPressed(bool noVR)
    {
        if (_repeater) //check if necessary or jus disable UI elements
        {
            OSCMessage message = new OSCMessage("/noVR");
            if (noVR) message.AddValue(OSCValue.Int(1));
            else message.AddValue(OSCValue.Int(0));
            _oscTransmitter.Send(message);
            Debug.Log("send No VR Button", DLogType.Network);    
        }
    }

    #endregion

    #region Private Methods

    private void ReceiveStartInstruction(OSCMessage message)
    {
        float value;
        Debug.Log("received start instruction");
        _receivedStartInstructionsGameEvent.Raise();
    }

    private void ReceiveNoVR(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
        {
            if (value == 0f)
            {
                Debug.Log("received VR on");
                _receiveNoVRGameEvent.Raise(false);    
            }
            else
            {
                Debug.Log("received no VR");
                _receiveNoVRGameEvent.Raise(true);   
            }
        }
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
            //TODO remove
            //LocalizationManager.instance.LoadLocalizedText(value);
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
                    if (message.Address == "/btn" + i) AudioManager.instance.GetComponent<AudioManager>().PlaySound(i);
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
    
    #endregion

}