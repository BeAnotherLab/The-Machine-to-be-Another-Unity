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
using UnityOSC;


public class oscControl : MonoBehaviour {
	
	public GameObject pointOfView;
	public GameObject audioManager;
	public Camera mainCamera;

	private string othersIP;
	private bool repeater;

	private Dictionary<string, ServerLog> servers;
	private Dictionary<string, ClientLog> clients;

	void Start() {	
		OSCHandler.Instance.Init("172.26.13.229", 8015); //init OSC
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
			// If we have received at least one packet,
			// show the last received from the log in the Debug console
			if(item.Value.log.Count > 0) 
			{
				int lastPacketIndex = item.Value.packets.Count - 1;
				
				UnityEngine.Debug.Log(String.Format("SERVER: {0} ADDRESS: {1} VALUE 0: {2}", 
				                                    item.Key, // Server name
				                                    item.Value.packets[lastPacketIndex].Address, // OSC address
				                                    item.Value.packets[lastPacketIndex].Data[0].ToString())); //First data value
				//recenter pose command
				if (item.Value.packets [lastPacketIndex].Address == "/ht" && item.Value.packets [lastPacketIndex].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().recenterPose ();
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/ht", 1);
					}
				} 
				//screen on/off command
				else if (item.Value.packets [lastPacketIndex].Address == "/dimon" && item.Value.packets [lastPacketIndex].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().setDimmed (false);
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/dimon", 1);
					}
				} else if (item.Value.packets [lastPacketIndex].Address == "/dimoff" && item.Value.packets [lastPacketIndex].Data [0].ToString () == "1") {
					pointOfView.GetComponent<webcam> ().setDimmed (true);
					if (repeater) {
						OSCHandler.Instance.SendMessageToClient ("sender", "/dimoff", 1);
					}
				}

				//pose data
				else if (item.Value.packets [lastPacketIndex].Address == "pose/x") {
					pointOfView.GetComponent<webcam> ().otherPose.x = (float) item.Value.packets [lastPacketIndex].Data [0];
				}
				else if (item.Value.packets [lastPacketIndex].Address == "pose/y") {
					pointOfView.GetComponent<webcam> ().otherPose.y = (float) item.Value.packets [lastPacketIndex].Data [0];
				}
				else if (item.Value.packets [lastPacketIndex].Address == "pose/z") {
					pointOfView.GetComponent<webcam> ().otherPose.z = (float) item.Value.packets [lastPacketIndex].Data [0];
				}
				else if (item.Value.packets [lastPacketIndex].Address == "pose/w") {
					pointOfView.GetComponent<webcam> ().otherPose.w = (float) item.Value.packets [lastPacketIndex].Data [0];
				}

				//position data
				else if (item.Value.packets [lastPacketIndex].Address == "position/x") {
					pointOfView.GetComponent<webcam> ().otherPosition.x = (float) item.Value.packets [lastPacketIndex].Data [0];
				}
				else if (item.Value.packets [lastPacketIndex].Address == "position/y") {
					pointOfView.GetComponent<webcam> ().otherPosition.y = (float) item.Value.packets [lastPacketIndex].Data [0];
				}
				else if (item.Value.packets [lastPacketIndex].Address == "position/z") {
					pointOfView.GetComponent<webcam> ().otherPosition.z = (float) item.Value.packets [lastPacketIndex].Data [0];
				}

				//audio clip trigger command
				else {
					for (int i = 0; i < audioManager.GetComponent<AudioPlayer> ().clips.Length; i++) {
						if (item.Value.packets [lastPacketIndex].Address == "/btn" + i.ToString () && item.Value.packets [lastPacketIndex].Data [0].ToString () == "1") {
							audioManager.GetComponent<AudioPlayer> ().playSound (i);
						}
					}
				}
			}
	    }
	}

	public void sendHeadTracking () {
		Quaternion q = mainCamera.transform.rotation;
		OSCHandler.Instance.SendMessageToClient ("sender", "/pose/x", q.x);
		OSCHandler.Instance.SendMessageToClient ("sender", "/pose/y", q.y);
		OSCHandler.Instance.SendMessageToClient ("sender", "/pose/z", q.z);
		OSCHandler.Instance.SendMessageToClient ("sender", "/pose/w", q.w);

		Vector3 p = mainCamera.transform.position;
		OSCHandler.Instance.SendMessageToClient ("sender", "/position/x", p.x);
		OSCHandler.Instance.SendMessageToClient ("sender", "/position/x", p.y);
		OSCHandler.Instance.SendMessageToClient ("sender", "/position/x", p.z);
	}
		
}