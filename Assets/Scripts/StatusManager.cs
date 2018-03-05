using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StatusManager : MonoBehaviour {

	public static bool thisUserIsReady = false;
	public static bool otherUserIsReady = false;

	private bool sessionIsPlaying = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	//	bool bothAreReady;

		CheckUsersStatus ();

		if (!sessionIsPlaying) {
			if (thisUserIsReady && otherUserIsReady) {
				startPlaying ();
				sessionIsPlaying = true;
			}
		}

		if (sessionIsPlaying) {
			if (!thisUserIsReady && otherUserIsReady || !otherUserIsReady && thisUserIsReady) {

			}

			if (!thisUserIsReady && !otherUserIsReady) sessionIsPlaying = false;
		}

	}

	void CheckUsersStatus (){

		//self
		if (XRDevice.userPresence == UserPresenceState.Present) thisUserIsReady = true;
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

		//other

	}



	void startPlaying(){

	}
}
