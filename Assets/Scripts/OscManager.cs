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
	
	public GameObject pointOfView;
	public GameObject audioManager;
	public Camera mainCamera;
	public GameObject gui;

	private bool previousStatusForSelf;

	public string othersIP = "";


	void Start() {	
		
		if (othersIP == null) othersIP = PlayerPrefs.GetString ("othersIP");

		initOSC();

	}

	private void initOSC (){
	}
		
	void Update() {
		sendHeadTracking ();
		//if(MenuButtonBAL.userIsReady) {
			if (StatusManager.thisUserIsReady != previousStatusForSelf) sendThisUserStatus(StatusManager.thisUserIsReady);
			previousStatusForSelf = StatusManager.thisUserIsReady;
		//}

	}

	public void sendHeadTracking () {
		
		Quaternion q = mainCamera.transform.rotation;

		/*
		Vector3 p = mainCamera.transform.position;
		message = new OscMessage ("/position");
		message.Add(p.x);
		message.Add(p.y);
		message.Add(p.z);
		oscOut.Send (message);*/

	}
		
	public void sendThisUserStatus(bool status){
		int i = 0;

		if (status == true) i = 1;
		else i = 0;

	}

	void receiveOtherUserStatus(){
		
		int x = 0;

		if (x == 0) StatusManager.otherUserIsReady = false;
		else if (x == 1) StatusManager.otherUserIsReady = true;
	}

	void receiveHeadTracking()
	{
		float x = 0, y = 0, z = 0, w = 0;
		pointOfView.GetComponent<webcam>().otherPose = new Quaternion (x, y, z, w);
	}

	void OnDisable() {
		PlayerPrefs.SetString("othersIP", othersIP);
	}

}