using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRStandardAssets.Menu;

public class StatusManager : MonoBehaviour {

	/// <summary>
	/// NOTE THAT THIS IS BEING CALLED TWICE, ONCE ON EACH SCENE, THE SOUNDS ARE MUTED FOR THE SECOND SCENE. THIS SHOULD BE FIXED
	/// </summary>
	 

	public static bool thisUserIsReady = false, otherUserIsReady = false;

	public bool fakeOculusReady; //for debugging
	public bool useWithLookAt;
	public bool useFakeOculus;

	public GameObject projectionScreen;
	public GameObject UICanvas;
	public Text messageInterfaceText;
	public webcam dimmer;
	public AudioManager audioManager;
	public AudioCoroutineCreator audioCoroutines;

	public float waitBeforeInstructions, waitAfterInstructionsForScreen, waitForMirror, waitForGoodbye;

	private bool sessionIsPlaying = false;
	private bool thisUserWasPlaying;
	private bool hasBeenCreated = false;

	void Start () {
		
		projectionScreen.SetActive (false);// = false;

		/*
		if (!hasBeenCreated) {
			DontDestroyOnLoad (this.gameObject);
			hasBeenCreated = true;
		}*/

	}


	void Update () {
		//if (MenuButtonBAL.userIsReady) {
			
			CheckThisUserStatus ();

			if (useFakeOculus)
				CheckForFakeOculus (); // for virtual other while debugging

			if (!sessionIsPlaying) {
				if (thisUserIsReady && otherUserIsReady)
					StartPlaying ();
				else if (thisUserIsReady && !otherUserIsReady)
					WaitingForOther ();
			}

			if (sessionIsPlaying) {
				if (!otherUserIsReady && thisUserIsReady)
					OtherIsGone ();
				if (!thisUserIsReady)
					StopExperience ();
			}

			if (!thisUserIsReady && thisUserWasPlaying)
				StopExperience ();//In case that the other user is never ready and this one stopped.


			if (Input.GetKeyDown ("o"))
				IsOver ();
		//}

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
		StartCoroutine ("MirrorCoroutine");

		yield return new WaitForFixedTime (waitBeforeInstructions);// wait before playing audio
			
		//audioManager.PlaySound ("instructions");
		audioCoroutines.StartAudioCoroutines ();

		yield return new WaitForFixedTime (waitAfterInstructionsForScreen);//duration of audio track to start video after

		UICanvas.SetActive (false);
		projectionScreen.SetActive (true);
		dimmer.setDimmed (true);

	}

	public IEnumerator GoodbyeCoroutine() {

		yield return new WaitForFixedTime (waitForGoodbye + waitBeforeInstructions);
		Debug.Log ("READY TO STOP");
		IsOver ();

	}

	public IEnumerator MirrorCoroutine() {

		yield return new WaitForFixedTime (waitBeforeInstructions + waitForMirror);
		Debug.Log ("READY FOR MIRROR");

	}

	void OtherIsGone () {//different than self is gone in case there is an audio for this case
		
		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		messageInterfaceText.text = LanguageTextDictionary.otherIsGone;

		CancelInvoke ("IsOver");
		StopAllCoroutines ();
		audioCoroutines.StopAudioCoroutines();
		//audioManager.StopAll ();

	}
		

	void StopExperience() {

		thisUserWasPlaying = false;

		dimmer.setDimmed (false);
		projectionScreen.SetActive (false);
		UICanvas.SetActive (true);
		sessionIsPlaying = false; 

		//CancelInvoke ("IsOver");
		//audioManager.StopAll ();
		audioCoroutines.StopAudioCoroutines();
		StopAllCoroutines ();
		messageInterfaceText.text = null;

		if(useWithLookAt) {
			MenuButtonBAL.userIsReady = false;
			SceneManager.LoadScene ("Look at interaction", LoadSceneMode.Single);
		}
		//Destroy(this.gameObject);
		
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
