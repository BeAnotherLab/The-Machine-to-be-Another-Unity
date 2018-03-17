//
//	 Based on - Example of usage for OSC receiver - Copyright (c) 2012 Jorge Garcia Martin

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
	
public class OscManager : MonoBehaviour {
	
	public GameObject pointOfView;
	public GameObject audioManager;
	public Camera mainCamera;
	public GameObject gui;

	public OscOut oscOut;
	public OscIn oscIn;
	private bool previousStatusForSelf;

	public string othersIP = "";


	void Start() {	
		
		if (othersIP == null) othersIP = PlayerPrefs.GetString ("othersIP");

		initOSC();

		oscIn.Map( "/pose", receiveHeadTracking );
		oscIn.Map("/otherUser", receiveOtherUserStatus);

	}

	private void initOSC (){
		if(oscOut == null) oscOut = gameObject.AddComponent<OscOut>();
		if(oscOut == null) oscIn = gameObject.AddComponent<OscIn>();

		oscOut.Open( 8015, othersIP);
		oscIn.Open(8015);
	}
		
	void Update() {
		sendHeadTracking ();

		if (StatusManager.thisUserIsReady != previousStatusForSelf) sendThisUserStatus(StatusManager.thisUserIsReady);
		previousStatusForSelf = StatusManager.thisUserIsReady;
	}

	public void sendHeadTracking () {
		
		Quaternion q = mainCamera.transform.rotation;
		OscMessage message = new OscMessage ("/pose");
		message.Add(q.x);
		message.Add(q.y);
		message.Add(q.z);
		message.Add(q.w);
		oscOut.Send (message);

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
		OscMessage message = new OscMessage ("/otherUser");

		if (status == true) i = 1;
		else i = 0;

		message.Add (i);
		oscOut.Send (message);

	}

	void receiveOtherUserStatus(OscMessage message){
		
		int x = 0;

		if (message.TryGet(0, out x)) {
			if (x == 0) StatusManager.otherUserIsReady = false;
			else if (x == 1) StatusManager.otherUserIsReady = true;
			Debug.Log ("received something");
		}
	}

	void receiveHeadTracking( OscMessage message )
	{
		float x = 0, y = 0, z = 0, w = 0;

		if( message.TryGet( 0, out x ) && message.TryGet( 1, out y ) && message.TryGet( 2, out z ) &&  message.TryGet( 3, out w ) ){
			pointOfView.GetComponent<webcam>().otherPose = new Quaternion (x, y, z, w);
		}

	}

	void OnDisable() {
		PlayerPrefs.SetString("othersIP", othersIP);
	}

}