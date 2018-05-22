using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StatusManager : MonoBehaviour {

	public static bool thisUserIsReady = false, otherUserIsReady = false;

	public bool fakeOculusReady; //for debugging
	public bool useWithLookAt;
	public bool useFakeOculus;

	public GameObject projectionScreen;
	public GameObject UICanvas;
	public Text messageInterfaceText;
	public webcam dimmer;
	public AudioManager audioManager;

	public float waitBeforeInstructions, waitAfterInstructionsForScreen, waitForGoodbye;

	private bool sessionIsPlaying = false;
	private bool thisUserWasPlaying;

	void Start () {
		
		projectionScreen.SetActive (false);// = false;

	}


	void Update () {

		CheckThisUserStatus ();

		if(useFakeOculus) CheckForFakeOculus (); // for virtual other while debugging

		if (!sessionIsPlaying) {
			if (thisUserIsReady && otherUserIsReady) StartPlaying ();
			else if (thisUserIsReady && !otherUserIsReady) WaitingForOther ();
		}

		if (sessionIsPlaying) {
			if (!otherUserIsReady && thisUserIsReady) OtherIsGone ();
			if (!thisUserIsReady) StopExperience ();
		}

		if (!thisUserIsReady && thisUserWasPlaying) StopExperience();//In case that the other user is never ready and this one stopped.


		if (Input.GetKeyDown ("o")) IsOver ();


	}

	void CheckThisUserStatus () {

		if (XRDevice.userPresence == UserPresenceState.Present) {
			thisUserIsReady = true;
			thisUserWasPlaying = true;
		}
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

	}


	void WaitingForOther () {
		
		messageInterfaceText.text = LanguageTextDictionary.waitForOther; 
		thisUserWasPlaying = true;

	}

	void StartPlaying () {

		messageInterfaceText.text = LanguageTextDictionary.instructions;
		sessionIsPlaying = true;
		StartCoroutine ("StartPlayingCoroutine");
	}

	public IEnumerator StartPlayingCoroutine() {

		StartCoroutine ("GoodbyeCoroutine");

		yield return new WaitForFixedTime (waitBeforeInstructions);// wait before playing audio
			
		audioManager.PlaySound ("instructions");

		yield return new WaitForFixedTime (waitAfterInstructionsForScreen);//duration of audio track to start video after

		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed (true);

	}

	public IEnumerator GoodbyeCoroutine() {

		yield return new WaitForFixedTime (waitForGoodbye + waitBeforeInstructions);
		IsOver ();

	}
		

	void OtherIsGone () {//different than self is gone in case there is an audio for this case
		
		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.otherIsGone;

		CancelInvoke ("IsOver");
		StopAllCoroutines ();
		audioManager.StopAll ();

	}
		

	void StopExperience() {

		thisUserWasPlaying = false;

		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		sessionIsPlaying = false; 

		//CancelInvoke ("IsOver");
		audioManager.StopAll ();
		StopAllCoroutines ();
		messageInterfaceText.text = null;

		if(useWithLookAt) SceneManager.LoadScene ("Look at interaction", LoadSceneMode.Single);
		
	}
		
	 
	void CheckForFakeOculus () {//only for debugging
		
		otherUserIsReady = fakeOculusReady;

	}


	void IsOver() {
		
		//audioManager.PlaySound ("goodbye");
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.finished;

	}
		
}
