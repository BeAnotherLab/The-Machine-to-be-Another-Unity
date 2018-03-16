using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class StatusManager : MonoBehaviour {

	public static bool thisUserIsReady = false;
	public static bool otherUserIsReady = false;

	public bool fakeOculusReady;

	private bool sessionIsPlaying = false, someoneIsGone = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	//	bool bothAreReady;

		CheckUsersStatus ();

		if (!sessionIsPlaying) {
			if (thisUserIsReady && otherUserIsReady) {
				Debug.Log ("both are ready, yay!");
				startPlaying ();
				sessionIsPlaying = true;
			} 

			else if (thisUserIsReady && !otherUserIsReady) {
				waitingForOther ();
			}
		}

		if (sessionIsPlaying){
			if (!someoneIsGone) {
				
			if (!thisUserIsReady && otherUserIsReady || !otherUserIsReady && thisUserIsReady) {
				Debug.Log ("woops, somebody stopped...");
				someoneIsGone = true;
			}
		}

			if (!thisUserIsReady && !otherUserIsReady) { 
				sessionIsPlaying = false; 
				Debug.Log ("now both stopped, come on!");
			}

		}

	}

	void CheckUsersStatus (){

		//self
		if (XRDevice.userPresence == UserPresenceState.Present) thisUserIsReady = true;
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

		//other
		checkForFakeOculus ();

	}


	void waitingForOther (){

	}

	void startPlaying (){

	}

	 
	void checkForFakeOculus (){
		otherUserIsReady = fakeOculusReady;
	}
}
