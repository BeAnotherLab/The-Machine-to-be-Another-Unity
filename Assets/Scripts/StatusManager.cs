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
	public AudioManager audioManager;

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

		//outside of the if statement because it may be the case that this happens after the session is over
		

	}

	void CheckThisUserStatus () {

		if (XRDevice.userPresence == UserPresenceState.Present) thisUserIsReady = true;
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

	}


	void WaitingForOther () {
		
		messageInterfaceText.text = LanguageTextDictionary.waitForOther;

	}

	void StartPlaying () {
		
		messageInterfaceText.text = LanguageTextDictionary.instructions;
		sessionIsPlaying = true;
		StartCoroutine ("ShowInstructionsForTime", 20f); //this should be set to the duration of the audio track
		audioManager.PlaySound ("instructions");

	}

	void OtherIsGone () {
		
		dimmer.setDimmed ();
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		CancelInvoke ("InstructionReminder");
		CancelInvoke ("IsOVer");
		messageInterfaceText.text = LanguageTextDictionary.otherIsGone;

	}

	void BothAreGone() {
		
		messageInterfaceText.text = null;
		sessionIsPlaying = false; 

	}
		
	 
	void CheckForFakeOculus () {//only for debugging

		otherUserIsReady = fakeOculusReady;
	
	}

	void InstructionReminder() {
		audioManager.PlaySound ("reminder");
	}

	void IsOver(){
		audioManager.PlaySound ("goodbye");
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.finished;
		CancelInvoke ("InstructionReminder");
	}

	public IEnumerator ShowInstructionsForTime(float time) {

		yield return new WaitForFixedTime (time);
		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed ();
		InvokeRepeating ("InstructionReminder", 10f, 10f);
		Invoke ("IsOver", 25f);
	}
}
