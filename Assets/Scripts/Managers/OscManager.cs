using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using VRStandardAssets.Menu;
using extOSC;

public class OscManager : MonoBehaviour {

    #region Public Fields

    public static OscManager instance;

    public string othersIP { get { return othersIP; } set { SetOthersIP(value); } }

    public bool sendHeadTracking;
    
    #endregion

    #region Private Fields

    private Camera _mainCamera;

    private OSCTransmitter _oscTransmitter;
    private OSCReceiver _oscReceiver;
    
    private bool _repeater;

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
        _oscReceiver.Bind("/pose", ReceivedHeadTracking);
        _oscReceiver.Bind("/otherUser", ReceivedOtherStatus);
        _oscReceiver.Bind("/dimon", ReceiveDimOn);
        _oscReceiver.Bind("/dimoff", ReceiveDimOff);
        _oscReceiver.Bind("/ht", ReceiveCalibrate);
        _oscReceiver.Bind("/serialStatus", ReceiveSerialStatus);
        for (int i = 0; i < 11; i++) _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

        //set IP address of other 
        SetOthersIP(PlayerPrefs.GetString("othersIP"));
    }

    private void Update()
    {
        SendHeadTracking();
    }

    #endregion

    #region Public Methods   

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

    public void SendHeadTracking()
    {
        if (sendHeadTracking)
        {
            Quaternion q = _mainCamera.transform.rotation;

            OSCMessage message = new OSCMessage ("/pose");
            var array = OSCValue.Array();

            array.AddValue(OSCValue.Float(q.x));
            array.AddValue(OSCValue.Float(q.y));
            array.AddValue(OSCValue.Float(q.z));
            array.AddValue(OSCValue.Float(q.w));

            message.AddValue(array);

            _oscTransmitter.Send(message);    
        }
    }

    public void SendThisUserStatus(bool status)
    {
        OSCMessage message = new OSCMessage("/otherUser");

        int i = 0;
        if (status) i = 1;

        message.AddValue(OSCValue.Int(i));
        _oscTransmitter.Send(message);
    }

    public void SendSerialStatus(bool status)
    {
        if (_repeater)
        {
            OSCMessage message = new OSCMessage("/serialStatus");

            int i = 0;
            if (status) i = 1;

            message.AddValue(OSCValue.Int(i));
            _oscTransmitter.Send(message);    
        }
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
            if (value == 1f) VideoFeed.instance.RecenterPose();

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) VideoFeed.instance.SetDimmed(true);

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOff(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) VideoFeed.instance.SetDimmed(false);

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveBtn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
        {
            if (value == 1f) {
                for (int i = 0; i < 11; i++)
                    if (message.Address == "/btn" + i.ToString()) AudioPlayer.instance.GetComponent<AudioPlayer>().playSound(i);
            }
        }
            

        if (_repeater) _oscTransmitter.Send(message);
    }

    private void ReceivedOtherStatus(OSCMessage message)
    {
        if (StatusManager.instance.statusManagementOn)
        {
            int x;
            if (message.ToInt(out x))
            {
                if (x == 0) StatusManager.instance.OtherLeft();
                else if (x == 1) StatusManager.instance.OtherUserIsReady();
            }
        }
    }

    private void ReceivedHeadTracking(OSCMessage message)
    {
        List<OSCValue> arrayValues = null;
        List<float> quaternionValues = new List<float>();

        if (message.ToArray(out arrayValues)){ // Get all values from first array in message.
            foreach (var value in arrayValues) 
                quaternionValues.Add(value.FloatValue); //add them to a float list
        }

        VideoFeed.instance.GetComponent<VideoFeed>().otherPose = new Quaternion(quaternionValues[0], quaternionValues[1], quaternionValues[2], quaternionValues[3]);
    }

    private void ReceiveSerialStatus(OSCMessage message)
    {
        if (StatusManager.instance.statusManagementOn)
        {
            int x;
            if (message.ToInt(out x))
            {
                if (x == 0) StatusManager.instance.SerialFailure();
                else if (x == 1) StatusManager.instance.SerialReady();
            }
        }
    }
    
    
    #endregion

}