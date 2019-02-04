//
//	 Based on - Example of usage for OSC receiver - Copyright (c) 2012 Jorge Garcia Martin

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
        _oscReceiver.Bind("/pose", ReceivedHeadTracking);
        _oscReceiver.Bind("/otherUser", ReceivedOtherStatus);

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

    public void SendHeadTracking()
    {
        Quaternion q = mainCamera.transform.rotation;

		OSCMessage message = new OSCMessage ("/pose");
		message.AddValue(OSCValue.Float(q.x));
		message.AddValue(OSCValue.Float(q.y));
		message.AddValue(OSCValue.Float(q.z));
		message.AddValue(OSCValue.Float(q.z));
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
        float x = 0, y = 0, z = 0, w = 0;
        if (message.ToFloat(out x) && message.ToFloat(out y) && message.ToFloat(out z))
            pointOfView.GetComponent<webcam>().otherPose = new Quaternion(x, y, z, w);
    }

    #endregion

}