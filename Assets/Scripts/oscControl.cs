//
//	  UnityOSC - Example of usage for OSC receiver
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
	
public class oscControl : MonoBehaviour {
	
	public GameObject pointOfView;
	public GameObject audioManager;
	public Camera mainCamera;
	public GameObject gui;
	public bool repeater;
	public OscOut oscOut;
	public OscIn oscIn;

	private string othersIP = "";

	//private Dictionary<string, ServerLog> servers;
	//private Dictionary<string, ClientLog> clients;

	void Start() {	
		othersIP = PlayerPrefs.GetString ("othersIP");
		if (othersIP != "") initOSC();
		if (PlayerPrefs.GetInt ("repeater") == 0) repeater = false;
		if (PlayerPrefs.GetInt ("repeater") == 1) repeater = true;
		oscIn.Map( "/pose", receiveHeadTracking );
		oscIn.Map( "/dimon", receiveDimOn );
		oscIn.Map( "/dimoff", receiveDimOff );
		oscIn.Map( "/ht", receiveCalibrate );

		for (int i = 0; i < 8; i++) {
			oscIn.Map ("/btn" + i.ToString(), receiveBtn); 
		}

	}

	private void initOSC (){
		othersIP = PlayerPrefs.GetString ("othersIP");
		if( !oscOut ) oscOut = gameObject.AddComponent<OscOut>();
		if( !oscIn ) oscIn = gameObject.AddComponent<OscIn>();

		oscOut.Open( 8015, othersIP);
		oscIn.Open(8015);
	}
		
	void Update() {
		sendHeadTracking ();
	}

	public void sendHeadTracking () {
		Quaternion q = mainCamera.transform.rotation;
		OscMessage message = new OscMessage ("/pose");
		message.args [0] = q.x;
		message.args [1] = q.y;
		message.args [2] = q.z;
		message.args [3] = q.w;
		oscOut.send (message);

		Vector3 p = mainCamera.transform.position;
		message = new OscMessage ("/position");
		message.args [0] = p.x;
		message.args [1] = p.y;
		message.args [2] = p.z;
		oscOut.send (message);

		List<object> position = new List<object> ();
		position.AddRange (new object[]{p.x, p.y, p.z});
		OSCHandler.Instance.SendMessageToClient ("sender", "/position", position);

	}

	public void toggleRepeater(bool r) {
		repeater = r;
		if (r) PlayerPrefs.SetInt ("repeater", 1);
		else   PlayerPrefs.SetInt ("repeater", 0);
	}

	public void setOthersIP (string ip){
		othersIP = ip;
		PlayerPrefs.SetString("othersIP", ip);
	}



	void receiveHeadTracking( OscMessage message )
	{
		// Get string arguments at index 0 and 1 safely.
		float x = 0;
		float y = 0;
		float z = 0;
		float w = 0;

		if( message.TryGet( 0, out x ) && message.TryGet( 1, out y ) && message.TryGet( 2, out z ) &&  message.TryGet( 3, out w ) ){
			Debug.Log( "Chino receive: " + x + " " + y + " " + z + " " + w );
		}

		pointOfView.GetComponent<webcam>().otherPose = new Quaternion (x, y, z, w);

	}

	void receiveCalibrate( OscMessage message) {
		float x = 0;
		if (message.TryGet (0, out x)) {
			pointOfView.GetComponent<webcam> ().recenterPose ();
			if (repeater) {
				oscOut.Send ("/ht", x);
			}
		}
	}

	void receiveDimOn(float value) {
		if (value == 1f) {
			pointOfView.GetComponent<webcam> ().setDimmed (false);
			if (repeater)	oscOut.Send ("/dimon", 1f);
		}
	}

	void receiveDimOff(float value) {
		if (value == 1f) {
			pointOfView.GetComponent<webcam> ().setDimmed (true);
			if (repeater) oscOut.Send ("/dimoff", 1f);
		}
	}

	void receiveBtn( OscMessage message ){
		for (int i = 0; i < 8; i++) {
			float x = 3;
				
			if (message.address == "/btn" + i.ToString ()){
				if (message.TryGet(0, out x)) {
					if (x == 1f) audioManager.GetComponent<AudioPlayer> ().playSound (i);
					if (repeater) {
						oscOut.Send("/btn" + i.ToString(), x);
					}
				}
			}
		}	
	}
}