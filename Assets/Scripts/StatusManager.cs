using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;

public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static StatusManager instance;

    public bool statusManagementOn;

    #endregion


    #region Private Fields
    
    [SerializeField] private bool _thisUserIsReady;
    [SerializeField] private bool _otherUserIsReady;
    [SerializeField] private bool _autoStartAndFinishOn;

    [Tooltip("Instructions Timing")] private GameObject _instructionsGUI;

    [SerializeField]
    private float waitBeforeInstructions, waitAfterInstructionsForScreen, waitForMirror, waitForGoodbye, waitForWall;
    //TODO make waitafterInstructions match the duration of the introduction audio

    private Text _instructionsText;

    private GameObject _mainCamera;

    private GameObject _confirmationMenu;

    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;
        _instructionsGUI = GameObject.Find("InstructionsGUI");
        _instructionsText = GameObject.Find("InstructionsText").GetComponent<Text>();

        _mainCamera = GameObject.Find("Main Camera");

        _confirmationMenu = GameObject.Find("ConfirmationMenu");
    }

    private void Start()
    {
        _instructionsText.text = LanguageTextDictionary.idle;
    }

    private void Update()
    {
        if ( statusManagementOn ) //status management is for both autonomous and manual swap
        {
            if (XRDevice.userPresence == UserPresenceState.NotPresent)
            {
                _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
                if (_thisUserIsReady) StopExperience(); //if we were ready and we took off the headset
            }

            if (Input.GetKeyDown("o"))
                IsOver();
        }
    }

    #endregion


    #region Public Methods

    public void SetAutoStartAndFinish(bool on, float waitTime = 5)
    {
        _autoStartAndFinishOn = on;
        waitBeforeInstructions = waitTime;
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        if (statusManagementOn) OscManager.instance.SendThisUserStatus(true);

        EnableConfirmationGUI(false); //hide status confirmation GUI elements

        //start experience or wait for the other if they're not ready yet
        if (_otherUserIsReady) StartPlaying();
        else _instructionsText.text = LanguageTextDictionary.waitForOther; //show GUI instruction that indicates to wait for the other

        _thisUserIsReady = true;
    }

    public void OtherUserIsReady() 
    {
        _otherUserIsReady = true;
        if (_thisUserIsReady) StartPlaying();
    }

    public void OtherLeft()
    {
        _otherUserIsReady = false;
        if (_thisUserIsReady)
        {
            //different than self is gone in case there is an audio for this case
            VideoFeed.instance.SetDimmed(true);

            _instructionsGUI.SetActive(true);
            _instructionsText.text = LanguageTextDictionary.otherIsGone;

            StopAllCoroutines();

            StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
        }
    }

    public void StopExperience()
    {
        VideoFeed.instance.SetDimmed(true);
        _instructionsGUI.SetActive(true);
        _instructionsText.text = LanguageTextDictionary.idle; 

        StopAllCoroutines();

        AudioPlayer.instance.StopAudioInstructions();

        //reset user status as it is not ready
        EnableConfirmationGUI(true);
        OscManager.instance.SendThisUserStatus(false);
        _thisUserIsReady = false;
    }

    public void DisableStatusManagement()
    {
        VideoFeed.instance.SetDimmed(true);
        _instructionsGUI.SetActive(false);
        _instructionsText.text = null;

        StopAllCoroutines();

        statusManagementOn = false;
    }
    
    #endregion


    #region Private Methods

    private IEnumerator StartPlayingCoroutine()
    {      
        if (_autoStartAndFinishOn) //if we are in auto swap
        {
            StartCoroutine("GoodbyeCoroutine");
            AudioPlayer.instance.PlayAudioInstructions();
        }

        StartCoroutine("MirrorCoroutine");
        StartCoroutine("WallCoroutine");

        yield return new WaitForFixedTime(waitBeforeInstructions);// wait before playing audio
        yield return new WaitForFixedTime(waitAfterInstructionsForScreen);//duration of audio track to start video after

        if (_autoStartAndFinishOn) //if we are in auto swap
        {
            ArduinoControl.instance.SendCommand("wal_on"); //close curtain
            ArduinoControl.instance.SendCommand("mir_off"); //hide mirror
        }

        if (_autoStartAndFinishOn) VideoFeed.instance.SetDimmed(false);
        _instructionsGUI.SetActive(false);
    }

    private IEnumerator GoodbyeCoroutine()
    {
        yield return new WaitForFixedTime(waitForGoodbye + waitBeforeInstructions);
        Debug.Log("READY TO STOP");
        IsOver();
    }

    private IEnumerator MirrorCoroutine()
    {
        yield return new WaitForFixedTime(waitBeforeInstructions + waitForMirror);
        Debug.Log("READY FOR MIRROR");
        ArduinoControl.instance.SendCommand("mir_on"); //show mirror
    }

    private IEnumerator WallCoroutine()
    {
        yield return new WaitForFixedTime(waitBeforeInstructions + waitForWall);
        Debug.Log("READY FOR WALL");
        ArduinoControl.instance.SendCommand("wal_off"); //open curtain
        ArduinoControl.instance.SendCommand("mir_off"); //hide mirror
    }
    
    private void EnableConfirmationGUI(bool enable)
    {
        ConfirmationButton.instance.gameObject.SetActive(enable);

        if (enable)
            _mainCamera.GetComponent<Reticle>().Show();
        else
        {
            _mainCamera.GetComponent<Reticle>().Hide();
            _mainCamera.GetComponent<CustomSelectionRadial>().Hide();
        }
    }

    private void StartPlaying()
    {
        _instructionsGUI.SetActive(true);
        _instructionsText.text = LanguageTextDictionary.instructions;
        StartCoroutine("StartPlayingCoroutine");
    }

    private void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.SetDimmed(true);

        _instructionsGUI.SetActive(true);
        _instructionsText.text = LanguageTextDictionary.finished;

        StopAllCoroutines();

        StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience
    }

    private IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        StopExperience();
    }

    #endregion
}
