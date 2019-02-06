using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRStandardAssets.Menu;

public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static bool thisUserIsReady = false, otherUserIsReady = false;

    public bool fakeOculusReady; //for debugging
    public bool useWithLookAt;
    public bool useFakeOculus;

    public GameObject projectionScreen;
    public GameObject UICanvas;
    public Text messageInterfaceText;
    public VideoFeed dimmer;
    public AudioManager audioManager;
    public AudioCoroutineCreator audioCoroutines;

    public float waitBeforeInstructions, waitAfterInstructionsForScreen, waitForMirror, waitForGoodbye, waitForWall;

    #endregion

    #region Private Fields

    private bool sessionIsPlaying = false;
    private bool thisUserWasPlaying;
    private bool hasBeenCreated = false;

    #endregion

    #region MonoBehaviour Methods

    void Start()
    {
        projectionScreen.SetActive(false);
    }

    void Update()
    {
        CheckThisUserStatus();

        if (useFakeOculus)
            CheckForFakeOculus(); // for virtual other while debugging

        if (!sessionIsPlaying)
        {
            if (thisUserIsReady && otherUserIsReady)
                StartPlaying();
            else if (thisUserIsReady && !otherUserIsReady)
                WaitingForOther();
        }
        else
        {
            if (!otherUserIsReady && thisUserIsReady)
                OtherIsGone();
            if (!thisUserIsReady)
                StopExperience();
        }

        if (!thisUserIsReady && thisUserWasPlaying)
            StopExperience();//In case that the other user is never ready and this one stopped.

        if (Input.GetKeyDown("o"))
            IsOver();

    }
    #endregion

    #region Public Methods
    #endregion

    private void CheckThisUserStatus () {

		//if(safeFakeOculusReady){
		if (XRDevice.userPresence == UserPresenceState.Present) {
			thisUserIsReady = true;
			thisUserWasPlaying = true;
		}
		else if (XRDevice.userPresence == UserPresenceState.NotPresent)	thisUserIsReady = false;

	}


	private void WaitingForOther () {
		
		messageInterfaceText.text = LanguageTextDictionary.waitForOther; 
		thisUserWasPlaying = true;

	}

	private void StartPlaying () {

		messageInterfaceText.text = LanguageTextDictionary.instructions;
		sessionIsPlaying = true;
		StartCoroutine ("StartPlayingCoroutine");
	}


    private void OtherIsGone()
    {//different than self is gone in case there is an audio for this case

        dimmer.setDimmed(false);
        projectionScreen.SetActive(false);
        UICanvas.SetActive(true);
        messageInterfaceText.text = LanguageTextDictionary.otherIsGone;

        CancelInvoke("IsOver");
        StopAllCoroutines();
        audioCoroutines.StopAudioCoroutines();
        audioManager.StopAll();

    }

    private void StopExperience()
    {

        thisUserWasPlaying = false;

        dimmer.setDimmed(false);
        projectionScreen.SetActive(false);
        UICanvas.SetActive(true);
        sessionIsPlaying = false;

        audioManager.StopAll();
        audioCoroutines.StopAudioCoroutines();
        StopAllCoroutines();
        messageInterfaceText.text = null;

        if (useWithLookAt)
        {
            MenuButtonBAL.userIsReady = false;
            SceneManager.LoadScene("Look at interaction", LoadSceneMode.Single);
        }

    }

    private void CheckForFakeOculus()
    {//only for debugging
        otherUserIsReady = fakeOculusReady;
    }

    private void IsOver()
    {
        projectionScreen.SetActive(false);
        UICanvas.SetActive(true);
        messageInterfaceText.text = LanguageTextDictionary.finished;
    }

    #region Private Methods
    #endregion

    public IEnumerator StartPlayingCoroutine() {

		StartCoroutine ("GoodbyeCoroutine");
		StartCoroutine ("MirrorCoroutine");
		StartCoroutine ("WallCoroutine");

		yield return new WaitForFixedTime (waitBeforeInstructions);// wait before playing audio
			
		audioManager.PlaySound ("instructions");
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

	public IEnumerator WallCoroutine() {

		yield return new WaitForFixedTime (waitBeforeInstructions + waitForWall);
		Debug.Log ("READY FOR WALL");
	}

}
