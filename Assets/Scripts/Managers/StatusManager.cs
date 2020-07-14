using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;
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
    [SerializeField] private PlayableDirector _instructionsTimeline;

    private GameObject _mainCamera;
    private bool _readyForStandby; //when we use serial, only go to standby if Arduino is ready.
    private GameObject _confirmationMenu;

    private bool _experienceStarted;
    
    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera");
        _confirmationMenu = GameObject.Find("ConfirmationMenu");
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate { SerialFailure(); });
    }

    private void Start()
    {
        Screen.fullScreen = true;
        InstructionsTextBehavior.instance.ShowTextFromKey("waitingForSerial");
        if (!SwapModeManager.instance.useCurtain)
        {
            _readyForStandby = true;
            ArduinoManager.instance.SetSerialControlComputer(false, false);
        }
    }

    private void Update()
    {
        if ( statusManagementOn ) //status management is for both autonomous and manual swap
        {
            if (XRDevice.userPresence == UserPresenceState.NotPresent && selfStatus != UserStatus.headsetOff) 
                SelfRemovedHeadset();
            else if (XRDevice.userPresence == UserPresenceState.Present && selfStatus == UserStatus.headsetOff) //if we just put the headset on 
                SelfPutHeadsetOn(); 
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

        EnableConfirmationGUI(false); 

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
        if (_experienceStarted) StartCoroutine(MessageGoneAndEndExperience()); //show "other is gone" message and stop experience
        else if (selfStatus == UserStatus.headsetOff) Standby();
        else InstructionsDisplay.instance.ShowWelcomeVideo();
    }

    public void Standby(bool start = false) //go back to initial state where users must read instructions before starting experience
    {
        Debug.Log("Standby");
        if (!start) VideoFeed.instance.SetDimmed(true); //TODO somehow this messses with Video Feed dimming when called on Start?
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        EnableConfirmationGUI(true); //show "ready" button
        if (_readyForStandby) ArduinoManager.instance.InitialPositions(); 
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
        _experienceStarted = false;

        Destroy(gameObject);
    }

    public void SerialReady(bool serialControlComputer = false)
    {
        if (serialControlComputer) ArduinoManager.instance.InitialPositions(); //if this computer is the one connected to the Arduino board
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        _readyForStandby = true;    
    }

    public void SelfRemovedHeadset()
    {
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more

        if (_experienceStarted) EndExperience(); //if experience had started, stop it
        else if(otherStatus == UserStatus.headsetOff) Standby(); //only go to standby if both headsets off
        selfStatus = UserStatus.headsetOff;
        OscManager.instance.SendThisUserStatus(selfStatus);
    }
    
    #endregion


    #region Private Methods
    
    private void HeadsetsOn()
    {
        Debug.Log("Headesets on");
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
            Debug.Log("Experience started");
            _instructionsTimeline.Play();
            _experienceStarted = true;
        }
    }

    private void EndExperience() //called at the the end of the experience
    {
        InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        DimAndStop();
    }

    private IEnumerator MessageGoneAndEndExperience(bool otherGone = false)
    {
        InstructionsTextBehavior.instance.ShowTextFromKey("otherIsGone", 3);
        DimAndStop();
        
        yield return new WaitForSeconds(3);
        
        InstructionsTextBehavior.instance.ShowTextFromKey("finished", 7);
    }
    
    private void DimAndStop()
    {
        VideoFeed.instance.SetDimmed(true);
        _instructionsTimeline.Stop();
        _experienceStarted = false;
        Debug.Log("finished");
    }
    
    #endregion
}