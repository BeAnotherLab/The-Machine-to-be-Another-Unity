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

	public float timeForHandsAudio, timeForObjectAudio, timeForMirrorAudio, timeForGoodbyeAudio;

	private bool sessionIsPlaying = false;
	private int reminderItem;

	void Start () {
		
		projectionScreen.SetActive (false);// = false;

	}


	void Update () {

		CheckThisUserStatus ();
		CheckForFakeOculus (); // for virtual other while debugging

		if (!sessionIsPlaying) {
			if (thisUserIsReady && otherUserIsReady) StartPlaying ();
			else if (thisUserIsReady && !otherUserIsReady) WaitingForOther ();
		}

		if (sessionIsPlaying) {
			if (!otherUserIsReady && thisUserIsReady) OtherIsGone ();
			if (!thisUserIsReady) SelfIsGone ();
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
		StartCoroutine ("StartPlayingCoroutine");
	}

	public IEnumerator StartPlayingCoroutine() {
		
		yield return new WaitForFixedTime (1f);// wait before playing audio
			
		audioManager.PlaySound ("instructions");

		yield return new WaitForFixedTime (2f);//duration of audio track to start video after

		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed (true);

		Invoke ("PlayHandSound", timeForHandsAudio);
		InvokeRepeating("RepeatingInstructions", 11f, 23 );
		Invoke ("PlayObjectSound", timeForObjectAudio);
		Invoke ("PlayMirrorSound", timeForGoodbyeAudio);
		Invoke ("IsOver", timeForGoodbyeAudio);

	}

	void PlayHandSound() {
		audioManager.PlaySound ("hands");
	}

	void PlayObjectSound() {
		audioManager.PlaySound ("object");
	}

	void PlayMirrorSound() {
		audioManager.PlaySound ("mirror");
	}
		
	void RepeatingInstructions() {
		audioManager.PlaySound ("reminder");
	}

	void OtherIsGone () {//different than self is gone in case there is an audio for this case
		
		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.otherIsGone;

		CancelInvoke ("InstructionReminder");
		CancelInvoke ("IsOver");
		StopAllCoroutines ();
		audioManager.StopAll ();

	}

	void SelfIsGone () {

		StopExperience ();

	}

	void BothAreGone() {

		StopExperience ();

	}

	void StopExperience() {

		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		sessionIsPlaying = false; 
		CancelInvoke ("InstructionReminder");
		CancelInvoke ("IsOver");
		audioManager.StopAll ();
		StopAllCoroutines ();
		reminderItem = 0;
		messageInterfaceText.text = null;

	}
		
	 
	void CheckForFakeOculus () {//only for debugging
		
		otherUserIsReady = fakeOculusReady;

	}


	void IsOver() {
		
		audioManager.PlaySound ("goodbye");
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.finished;
		CancelInvoke ("InstructionReminder");

	}
		
}
