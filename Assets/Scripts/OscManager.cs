//
//	 Based on - Example of usage for OSC receiver - Copyright (c) 2012 Jorge Garcia Martin

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using VRStandardAssets.Menu;
	
public class OscManager : MonoBehaviour {

    #region Public Fields

    public GameObject pointOfView;
    public GameObject audioManager;
    public Camera mainCamera;
    public GameObject gui;
    public string othersIP = "";

    #endregion

    #region Private Fields

    private bool previousStatusForSelf;

    #endregion

    #region MonoBehaviour Methods

    private void Start()
    {
        if (othersIP == null) othersIP = PlayerPrefs.GetString("othersIP");
        InitOSC();
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

        /*
		Vector3 p = mainCamera.transform.position;
		message = new OscMessage ("/position");
		message.Add(p.x);
		message.Add(p.y);
		message.Add(p.z);
		oscOut.Send (message);*/

    }

    public void SendThisUserStatus(bool status)
    {
        int i = 0;

        if (status == true) i = 1;
        else i = 0;

    }

    #endregion


    #region Private Methods

    private void InitOSC()
    {
    }

    private void ReceiveOtherStatus()
    {
        int x = 0;
        if (x == 0) StatusManager.otherUserIsReady = false;
        else if (x == 1) StatusManager.otherUserIsReady = true;
    }

    void receiveHeadTracking()
    {
        float x = 0, y = 0, z = 0, w = 0;
        pointOfView.GetComponent<webcam>().otherPose = new Quaternion(x, y, z, w);
    }

    #endregion

}