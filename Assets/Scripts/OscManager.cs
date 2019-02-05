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

    public bool repeater;

    //TODO turn into singletons
    public GameObject pointOfView;
    public GameObject audioManager;
    public Camera mainCamera;
    public GameObject gui;
    public string othersIP = "";

    #endregion

    #region Private Fields

    private OSCTransmitter _oscTransmitter;
    private OSCReceiver _oscReceiver;

    private bool previousStatusForSelf;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
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
        for (int i = 0; i < 11; i++)
            _oscReceiver.Bind("/btn" + i.ToString(), ReceiveBtn);

        //TODO set IP and port from playerprefs to oscTransmitter
        if (othersIP == null) othersIP = PlayerPrefs.GetString("othersIP");
    }

    private void Update()
    {
        SendHeadTracking();
        //if(MenuButtonBAL.userIsReady) {
        if (StatusManager.thisUserIsReady != previousStatusForSelf) SendThisUserStatus(StatusManager.thisUserIsReady);
        previousStatusForSelf = StatusManager.thisUserIsReady;
        //}
    }

    private void OnDisable()
    {
        PlayerPrefs.SetString("othersIP", othersIP);
    }

    #endregion

    #region Public Methods

    public void SetRepeater(bool r)
    {
        repeater = r;
        if (r) PlayerPrefs.SetInt("repeater", 1);
        else PlayerPrefs.SetInt("repeater", 0);
    }

    public void SendHeadTracking()
    {
        Quaternion q = mainCamera.transform.rotation;

		OSCMessage message = new OSCMessage ("/pose");
        var array = OSCValue.Array();

		array.AddValue(OSCValue.Float(q.x));
        array.AddValue(OSCValue.Float(q.y));
        array.AddValue(OSCValue.Float(q.z));
        array.AddValue(OSCValue.Float(q.w));

        message.AddValue(array);

        _oscTransmitter.Send(message);
    }

    public void SendThisUserStatus(bool status)
    {
        OSCMessage message = new OSCMessage("/otherUser");

        int i = 0;
        if (status == true) i = 1;

        message.AddValue(OSCValue.Int(i));
        _oscTransmitter.Send(message);
    }

    #endregion

    #region Private Methods

    private void ReceiveCalibrate(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) pointOfView.GetComponent<webcam>().recenterPose();

        if (repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) pointOfView.GetComponent<webcam>().setDimmed(true);

        if (repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveDimOff(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
            if (value == 1f) pointOfView.GetComponent<webcam>().setDimmed(false);

        if (repeater) _oscTransmitter.Send(message);
    }

    private void ReceiveBtn(OSCMessage message)
    {
        float value;
        if (message.ToFloat(out value))
        {
            if (value == 1f) {
                for (int i = 0; i < 11; i++)
                    if (message.Address == "/btn" + i.ToString()) audioManager.GetComponent<AudioPlayer>().playSound(i);
            }
        }
            

        if (repeater) _oscTransmitter.Send(message);
    }

    private void ReceivedOtherStatus(OSCMessage message)
    {
        int x;
        if (message.ToInt(out x))
        {
            if (x == 0) StatusManager.otherUserIsReady = false;
            else if (x == 1) StatusManager.otherUserIsReady = true;
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

        pointOfView.GetComponent<webcam>().otherPose = new Quaternion(quaternionValues[0], quaternionValues[1], quaternionValues[2], quaternionValues[3]);
    }

    #endregion

}