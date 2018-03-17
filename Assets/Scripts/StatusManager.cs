using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

public class StatusManager : MonoBehaviour {

	public static bool thisUserIsReady = false, otherUserIsReady = false;

	public bool fakeOculusReady; //for debugging

	public GameObject projectionScreen;
	public GameObject UICanvas;
	public Text messageInterfaceText;
	public webcam dimmer;

	private bool sessionIsPlaying = false;

	void Start () {
		projectionScreen.SetActive (false);// = false;
	}


	void Update () {

		CheckThisUserStatus ();
		//CheckForFakeOculus (); // for virtual other while debugging

		if (!sessionIsPlaying) {
			if (thisUserIsReady && otherUserIsReady) StartPlaying ();
			else if (thisUserIsReady && !otherUserIsReady) WaitingForOther ();
		}

		if (sessionIsPlaying){
			if (!otherUserIsReady && thisUserIsReady) OtherIsGone ();
			if (!thisUserIsReady && !otherUserIsReady) BothAreGone();
		}
		

	}

	void CheckThisUserStatus () {

		if (XRDevice.userPresence == UserPresenceState.Present) thisUserIsReady = true;
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

	}


	void WaitingForOther () {
		
		messageInterfaceText.text = LanguageDictionary.waitForOther;

	}

	void StartPlaying () {
		
		messageInterfaceText.text = LanguageDictionary.instructions;
		sessionIsPlaying = true;
		StartCoroutine ("ShowInstructionsForTime", 5f);

	}

	void OtherIsGone () {
		
		dimmer.setDimmed ();
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageDictionary.otherIsGone;

	}

	void BothAreGone() {
		
		messageInterfaceText.text = null;
		sessionIsPlaying = false; 

	}
		
	 
	void CheckForFakeOculus () {//only for debugging

		otherUserIsReady = fakeOculusReady;
	
	}

	public IEnumerator ShowInstructionsForTime(float time) {

		yield return new WaitForFixedTime (time);
		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed ();

	}
}
