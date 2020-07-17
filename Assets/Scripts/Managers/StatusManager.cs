using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;
using UnityEngine.UI;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;
using Uduino;

public enum UserStatus { headsetOff, headsetOn, readyToStart }

public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static StatusManager instance;

    public bool statusManagementOn;

    public UserStatus selfStatus;
    public UserStatus otherStatus;
    
    #endregion


    #region Private Fields

    [SerializeField] private bool _autoStartAndFinishOn; //TODO check if can remove
    
    [SerializeField] private PlayableDirector _shortTimeline;
    [SerializeField] private PlayableDirector _longTimeline;
    private PlayableDirector _instructionsTimeline;

    private GameObject _mainCamera;
    private bool _readyForStandby; //when we use serial, only go to standby if Arduino is ready.
    private GameObject _confirmationMenu;
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera");
        _confirmationMenu = GameObject.Find("ConfirmationMenu");
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate { SerialFailure(); });
        _instructionsTimeline = _shortTimeline; //use short experience by default
    }

    private void Start()
    {
        InstructionsTextBehavior.instance.ShowTextFromKey("waitingForSerial");
    }

    private void Update()
    {
        if ( statusManagementOn ) //status management is for both autonomous and manual swap
        {
            if (XRDevice.userPresence == UserPresenceState.NotPresent && selfStatus != UserStatus.headsetOff) 
                SelfRemovedHeadset();
            else if (XRDevice.userPresence == UserPresenceState.Present && selfStatus == UserStatus.headsetOff) //if we just put the headset on 
                SelfPutHeadsetOn(); 
            
            if (Input.GetKeyDown("o")) IsOver();
        } 
    }

    #endregion


    #region Public Methods

    public void StartExperience()
    {
        if (_autoStartAndFinishOn) //if we are in auto swap
        {
            ArduinoManager.instance.SendCommand("wal_on"); //close curtain
            ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        }
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        if (_autoStartAndFinishOn) VideoFeed.instance.SetDimmed(false);
    }

    public void MirrorOn()
    {
        ArduinoManager.instance.SendCommand("mir_on");
    }

    public void WallOn()
    {
        ArduinoManager.instance.SendCommand("wal_off"); //open curtain
        ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
    }
    
    public void SetAutoStartAndFinish(bool on, float waitTime = 5)
    {
        _autoStartAndFinishOn = on;
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        if (statusManagementOn) OscManager.instance.SendThisUserStatus(UserStatus.readyToStart);

        EnableConfirmationGUI(false); //hide status confirmation GUI elements

        //start experience or wait for the other if they're not ready yet
        if (otherStatus == UserStatus.readyToStart) StartPlaying();
        else InstructionsTextBehavior.instance.ShowTextFromKey("waitForOther");

        selfStatus = UserStatus.readyToStart;
    }

    public void OtherUserIsReady()
    {
        otherStatus = UserStatus.readyToStart;
        if (selfStatus == UserStatus.readyToStart) StartPlaying();
    }

    public void SelfPutHeadsetOn()
    {
        selfStatus = UserStatus.headsetOn;
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        OscManager.instance.SendThisUserStatus(UserStatus.headsetOn);
        if (otherStatus == UserStatus.headsetOn) HeadsetsOn();    
    }

    public void OtherPutHeadsetOn()
    {
        otherStatus = UserStatus.headsetOn;
        if (selfStatus == UserStatus.headsetOn) HeadsetsOn();
    }
    
    public void OtherLeft()
    {
        otherStatus = UserStatus.headsetOff;
        if (selfStatus == UserStatus.readyToStart)
        {
            //different than self is gone in case there is an audio for this case
            VideoFeed.instance.SetDimmed(true);

            InstructionsTextBehavior.instance.ShowTextFromKey("otherIsGone");
            _instructionsTimeline.Stop();
            StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
        }
        else
        {
            InstructionsDisplay.instance.ShowWelcomeVideo();
        }
    }

    public void Standby(bool start = false)
    {
        if (!start) VideoFeed.instance.SetDimmed(true); //TODO somehow this messses with Video Feed dimming when called on Start?
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");

        _instructionsTimeline.Stop();
        AudioPlayer.instance.StopAudioInstructions();

        //reset user status as it is not ready
        EnableConfirmationGUI(true);

        if (_readyForStandby) //TODO is check necessary? 
            ArduinoManager.instance.InitialPositions();

        InstructionsDisplay.instance.ShowWelcomeVideo();
    }

    public void DisableStatusManagement()
    {
        VideoFeed.instance.SetDimmed(true);
        InstructionsTextBehavior.instance.ShowInstructionText(false);

        statusManagementOn = false;
    }

    public void SerialFailure() //if something went wrong with the physical installation
    {
        VideoFeed.instance.SetDimmed(true);
        OscManager.instance.SendSerialStatus(false);
        AudioPlayer.instance.StopAudioInstructions();    
        InstructionsDisplay.instance.ShowTechnicalFailureMessage();
        InstructionsTextBehavior.instance.ShowTextFromKey("systemFailure");
        _instructionsTimeline.Stop();
        Destroy(gameObject);
    }

    public void SerialReady(bool serialControlComputer = false)
    {
        if (serialControlComputer) //if this computer is the one connected to the Arduino board
        {
            ArduinoManager.instance.InitialPositions();
        }
        
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        _readyForStandby = true;    
    }

    public void SelfRemovedHeadset()
    {
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (selfStatus == UserStatus.readyToStart) Standby(); //if we were ready and we took off the headset
        if (selfStatus == UserStatus.headsetOn) InstructionsDisplay.instance.ShowWelcomeVideo(); //if we just had headset on
        selfStatus = UserStatus.headsetOff;
        OscManager.instance.SendThisUserStatus(selfStatus);
    }
    
    public void SetInstructionsTimeline(int index)
    {
        if (index == 0)
            _instructionsTimeline = _shortTimeline;
        else if (index == 1)
            _instructionsTimeline = _longTimeline;
    }

    
    #endregion


    #region Private Methods
    
    private void HeadsetsOn()
    {
        InstructionsDisplay.instance.ShowWaitForTurnVideo();
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
        if (_readyForStandby)
        {
            InstructionsTextBehavior.instance.ShowTextFromKey("instructions");
            _instructionsTimeline.Play();
        }
    }

    private void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.SetDimmed(true);
        InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        _instructionsTimeline.Stop();
    }

    private IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby();
    }

    #endregion
}
 