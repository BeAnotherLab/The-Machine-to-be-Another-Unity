using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VR;

public class HeadLog : MonoBehaviour {

	public float logRate;
	public Camera viewForHeadTracking;

	//private string condition;

	private float lastRotationMagnitude;
	private float currentRotationAcceleration;

	private Vector3 cameraRotation;

	private int participantID = 0;
	private float startTimeForParticipant;

	// Use this for initialization
	void Start () {
		
	}


	public void StartWriting() {
		participantID = participantID + 1;
		WriteToFile ("subject ID", "date", "pitch", "yaw", "roll", "angular acceleration", "time stamp");
		startTimeForParticipant = Time.fixedTime;
		InvokeRepeating ("FastLogger", 0.0f, logRate);
	}

	public void StopWriting() {
		CancelInvoke ("FastLogger");
		Debug.Log ("finished writing shit");
	}

	// Update is called once per frame
	void Update () {

		if (viewForHeadTracking != null) {
			//For oculus x is pitch, y is yaw, and z is roll 
			cameraRotation = viewForHeadTracking.transform.rotation.eulerAngles;
			Vector3 orientationVector = viewForHeadTracking.transform.rotation.eulerAngles;
			currentRotationAcceleration = orientationVector.magnitude - lastRotationMagnitude;
			lastRotationMagnitude = orientationVector.magnitude;
		} 

		else if (viewForHeadTracking = null) {
			cameraRotation = new Vector3 (0, 0, 0);
			currentRotationAcceleration = 0;
		}

	}

	void FastLogger (){
		//Debug.Log ("the pitch is " + cameraRotation.x.ToString() + ", the yaw is " + cameraRotation.y.ToString() + ", the roll is " + cameraRotation.z.ToString());

		WriteToFile (participantID.ToString() +  PlayerPrefs.GetString("participantID"), System.DateTime.Now.ToString(), cameraRotation.x.ToString(), cameraRotation.y.ToString(), 
			cameraRotation.z.ToString(), currentRotationAcceleration.ToString(), (Time.fixedTime - startTimeForParticipant).ToString());

	}


	void WriteToFile(string a, string b, string c, string d, string e, string  f, string g) {

		string date = DateTime.Now.ToString ("g");
		date = date.Replace ("/", "_");
		date = date.Replace (":", "_");
		string ip = PlayerPrefs.GetString ("othersIP");
		ip = ip.Replace (".", "");

		Debug.Log (date);
		string stringLine =  a + "," + b + "," + c + "," + d + "," + e + "," + f + "," + g;

		System.IO.StreamWriter file = new System.IO.StreamWriter ("./Logs/" + participantID.ToString() + PlayerPrefs.GetString("participantID") + "_" + date +".csv", true);
		file.WriteLine(stringLine);
		file.Close();	
	}
}
