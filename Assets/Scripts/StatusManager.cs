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

		if (sessionIsPlaying) {
			if (!otherUserIsReady && thisUserIsReady) SomeoneIsGone ();
			if (!thisUserIsReady) SomeoneIsGone ();
		}

		if (!thisUserIsReady && !otherUserIsReady) BothAreGone();


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
		//StartCoroutine ("ShowInstructionsForTime", 20f); //this should be set to the duration of the audio track
		StartCoroutine ("StartPlayingCoroutine");
	}

	public IEnumerator StartPlayingCoroutine(){
		
		yield return new WaitForFixedTime (3);// wait before playing audio
			
		audioManager.PlaySound ("instructions");

		yield return new WaitForFixedTime (21);//duration of audio track to start video after

		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed (true);
		InvokeRepeating ("InstructionReminder", 20f, 20f);
		Invoke ("IsOver", 280f);
	}

	void SomeoneIsGone () {
		
		dimmer.setDimmed ();
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		CancelInvoke ("InstructionReminder");
		CancelInvoke ("IsOver");
		messageInterfaceText.text = LanguageTextDictionary.otherIsGone;
		audioManager.StopAll ();

	}

	void BothAreGone() {
		
		projectionScreen.SetActive (false);
		messageInterfaceText.text = null;
		sessionIsPlaying = false; 
		audioManager.StopAll ();
		StopAllCoroutines ();
	}
		
	void InstructionReminder() {

		audioManager.PlaySound ("reminder");

	}
	 
	void CheckForFakeOculus () {//only for debugging
		
		otherUserIsReady = fakeOculusReady;

	}


	void IsOver(){
		
		audioManager.PlaySound ("goodbye");
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.finished;
		CancelInvoke ("InstructionReminder");

	}
		
}
