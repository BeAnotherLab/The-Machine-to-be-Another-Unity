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
using UnityOSC;


public class oscControl : MonoBehaviour {
	
	public GameObject pointOfView;
	public GameObject audioManager;
	public Camera mainCamera;

	private string othersIP = "";
	private bool repeater = false;

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	void Start() {	
		othersIP = PlayerPrefs.GetString ("othersIP");
		if (othersIP != "") initOSC();
		if (PlayerPrefs.GetInt ("repeater") == 0) repeater = false;
		if (PlayerPrefs.GetInt ("repeater") == 1) repeater = true;
	}

	private void initOSC (){
		OSCHandler.Instance.Init(othersIP, 8015); //init OSC
		servers = new Dictionary<string, ServerLog>();
		clients = new Dictionary<string, ClientLog>();
	}

	// NOTE: The received messages at each server are updated here
    // Hence, this update depends on your application architecture
    // How many frames per second or Update() calls per frame?
	void Update() {
		
		OSCHandler.Instance.UpdateLogs();
		servers = OSCHandler.Instance.Servers;

		sendHeadTracking ();

		foreach( KeyValuePair<string, ServerLog> item in servers )
		{
			// If we have received at least one packet
			// process the last one
			int i = item.Value.packets.Count - 1;

			if(item.Value.log.Count > 0) 
			{
				//recenter pose command
				if (item.Value.packets [i].Address == "/ht" && item.Value.packets [i].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().recenterPose ();
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/ht", 1);
					}
				} 

				//screen on/off command
				else if (item.Value.packets [i].Address == "/dimon" && item.Value.packets [i].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().setDimmed (false);
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/dimon", 1);
					}
				} else if (item.Value.packets [i].Address == "/dimoff" && item.Value.packets [i].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().setDimmed (true);
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/dimoff", 1);
					}
				}

				//pose data
				else if (item.Value.packets [i].Address == "/pose") {
					pointOfView.GetComponent<webcam> ().nextOtherPose = new Quaternion (
						(float)item.Value.packets [i].Data [0],
						(float)item.Value.packets [i].Data [1],
						(float)item.Value.packets [i].Data [2],
						(float)item.Value.packets [i].Data [3]);
				}

				//position data
				else if (item.Value.packets [i].Address == "/position") {
					pointOfView.GetComponent<webcam> ().otherPosition = new Vector3 (
						(float) item.Value.packets [i].Data [0],
						(float) item.Value.packets [i].Data [1],
						(float) item.Value.packets [i].Data [2]);
				}

				//audio clip trigger command
				else {
					for (int j = 0; j < audioManager.GetComponent<AudioPlayer> ().clips.Length; j++) {
						if (item.Value.packets[i].Address == "/btn" + j.ToString () && item.Value.packets[i].Data [0].ToString () == "1") {
							audioManager.GetComponent<AudioPlayer> ().playSound (j);
						}
					}
				}
			}
	    }
	}

	public void sendHeadTracking () {
		Quaternion q = mainCamera.transform.rotation;
		List<object> pose = new List<object> ();
		pose.AddRange(new object[]{q.x, q.y, q.z, q.w});
		OSCHandler.Instance.SendMessageToClient ("sender", "/pose", pose);
		/*
		Vector3 p = mainCamera.transform.position;
		List<object> position = new List<object> ();
		position.AddRange (new object[]{p.x, p.y, p.z});
		OSCHandler.Instance.SendMessageToClient ("sender", "/position", position);
*/	
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
}