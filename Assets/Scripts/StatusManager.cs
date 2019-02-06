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

    #endregion


    #region Private Fields

    [SerializeField]
    private bool _sesssionIsPlaying = false;
    [SerializeField]
    private bool _thisUserWasPlaying;

    [SerializeField]
    private bool _fakeOculusReady; //for debugging
    [SerializeField]
    private bool _useFakeOculus;

    private VideoFeed _videoFeed;
    private GameObject _instructionsGUI;

    [Tooltip("Instructions Timing")]

    [SerializeField]
    private float waitBeforeInstructions, waitAfterInstructionsForScreen, waitForMirror, waitForGoodbye, waitForWall;

    private Text _instructionsText;

    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        _videoFeed = FindObjectOfType<VideoFeed>();
        _instructionsText = GameObject.Find("InstructionsText").GetComponent<Text>();
    }

    private void Update() //TODO use events instead of polling status in Update() to make state transitions clearer
    {
        CheckThisUserStatus();

        if (_useFakeOculus)
            CheckForFakeOculus(); // for virtual other while debugging

        if (!_sesssionIsPlaying)
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

        if (!thisUserIsReady && _thisUserWasPlaying)
            StopExperience();//In case that the other user is never ready and this one stopped.

        if (Input.GetKeyDown("o"))
            IsOver();

    }
    #endregion


    #region Public Methods

    public IEnumerator StartPlayingCoroutine()
    {

        StartCoroutine("GoodbyeCoroutine");
        StartCoroutine("MirrorCoroutine");
        StartCoroutine("WallCoroutine");

        yield return new WaitForFixedTime(waitBeforeInstructions);// wait before playing audio

        yield return new WaitForFixedTime(waitAfterInstructionsForScreen);//duration of audio track to start video after

        _instructionsGUI.SetActive(false);
        _videoFeed.setDimmed(true);

    }

    public IEnumerator GoodbyeCoroutine()
    {

        yield return new WaitForFixedTime(waitForGoodbye + waitBeforeInstructions);
        Debug.Log("READY TO STOP");
        IsOver();

    }

    public IEnumerator MirrorCoroutine()
    {

        yield return new WaitForFixedTime(waitBeforeInstructions + waitForMirror);
        Debug.Log("READY FOR MIRROR");

    }

    public IEnumerator WallCoroutine()
    {

        yield return new WaitForFixedTime(waitBeforeInstructions + waitForWall);
        Debug.Log("READY FOR WALL");
    }
    #endregion


    #region Private Methods

    private void CheckThisUserStatus()
    {

        //if(safeFakeOculusReady){
        if (XRDevice.userPresence == UserPresenceState.Present)
        {
            thisUserIsReady = true;
            _thisUserWasPlaying = true;
        }
        else if (XRDevice.userPresence == UserPresenceState.NotPresent) thisUserIsReady = false;

    }

    private void WaitingForOther()
    {
        _instructionsText.text = LanguageTextDictionary.waitForOther;
        _thisUserWasPlaying = true;
    }

    private void StartPlaying()
    {
        _instructionsText.text = LanguageTextDictionary.instructions;
        _sesssionIsPlaying = true;
        StartCoroutine("StartPlayingCoroutine");
    }

    private void OtherIsGone() //different than self is gone in case there is an audio for this case
    {
        _videoFeed.setDimmed(false);
        _instructionsGUI.SetActive(true);
        _instructionsText.text = LanguageTextDictionary.otherIsGone;

        CancelInvoke("IsOver");
        StopAllCoroutines();

    }

    private void StopExperience()
    {
        _thisUserWasPlaying = false;
        _videoFeed.setDimmed(false);
        _instructionsGUI.SetActive(true);
        _sesssionIsPlaying = false;

        StopAllCoroutines();
        _instructionsText.text = null;
    }

    private void CheckForFakeOculus() //only for debugging
    {
        otherUserIsReady = _fakeOculusReady;
    }

    private void IsOver()
    {
        _instructionsGUI.SetActive(true);
        _instructionsText.text = LanguageTextDictionary.finished;
    }

    #endregion
}
