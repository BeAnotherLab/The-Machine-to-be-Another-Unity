using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.XR;
using UnityEngine.UI;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;
using Uduino;
using Debug = DebugFile;

public enum UserStatus { headsetOff, headsetOn, readyToStart } 

public class StatusManager : MonoBehaviour {

    #region Public Fields

    public static StatusManager instance;

    [HideInInspector] public bool presenceDetection;

    public UserStatus selfStatus;
    public UserStatus otherStatus;
    public PlayableDirector instructionsTimeline;
    
    #endregion


    #region Private Fields
    
    [SerializeField] private PlayableDirector _shortTimeline;
    [SerializeField] private PlayableDirector _longTimeline;
    [SerializeField] private GameObject _languageButtons;
    
    private GameObject _mainCamera;
    private bool _readyForStandby; //when we use serial, only go to standby if Arduino is ready.
    private GameObject _confirmationMenu;
    private bool _experienceRunning;
    private bool _dimOutOnExperienceStart;

    #endregion


    #region MonoBehaviour Methods

    private void Awake()
    {
        if (instance == null) instance = this;

        _mainCamera = GameObject.Find("Main Camera");
        _confirmationMenu = GameObject.Find("ConfirmationMenu");
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate { SerialFailure(); });
        instructionsTimeline = _longTimeline; //use short experience by default
    }

    private void Start()
    {
        if (SwapModeManager.instance.ArduinoControl)
            InstructionsTextBehavior.instance.ShowTextFromKey("waitingForSerial");
        else
            _readyForStandby = true; //if we're not using the serial control, we don't have to wait for the arduino
    }

    private void Update()
    {
        if (presenceDetection) //presence is for both autonomous and manual swap
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
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        if (_dimOutOnExperienceStart) VideoFeed.instance.Dim(false);
        else instructionsTimeline.Stop();
        Debug.Log("experience started");
    }

    public void MirrorOn()
    {
        //ArduinoManager.instance.SendCommand("mir_on");
        Debug.Log("mirrors on");
    }

    public void CloseWall()
    {
        ArduinoManager.instance.WallOn(true);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall on");        
    }
    
    public void WallOn() //TODO rename
    {
        OpenCurtainCanvasController.instance.Show("Open Curtain");
        ArduinoManager.instance.WallOn(false);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall off");
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        if (presenceDetection) OscManager.instance.SendThisUserStatus(UserStatus.readyToStart);

        EnableConfirmationGUI(false); //hide status confirmation GUI elements
        _languageButtons.gameObject.SetActive(false); //hide language buttons;

        //start experience or wait for the other if they're not ready yet
        if (otherStatus == UserStatus.readyToStart) StartPlaying();
        else InstructionsTextBehavior.instance.ShowTextFromKey("waitForOther");

        selfStatus = UserStatus.readyToStart;
        Debug.Log("this user is ready", DLogType.Input);
    }

    public void OtherUserIsReady()
    {
        otherStatus = UserStatus.readyToStart;
        if (selfStatus == UserStatus.readyToStart) StartPlaying();
        Debug.Log("the other user is ready", DLogType.Input);
    }

    public void SelfPutHeadsetOn()
    {
        selfStatus = UserStatus.headsetOn;
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        OscManager.instance.SendThisUserStatus(UserStatus.headsetOn);
        Debug.Log("this user put on the headset", DLogType.Input);
    }

    public void OtherPutHeadsetOn()
    {
        otherStatus = UserStatus.headsetOn;
        Debug.Log("the other user put on the headset", DLogType.Input);
    }
    
    public void OtherLeft()
    {
        otherStatus = UserStatus.headsetOff;
        //if experience started
        if (selfStatus == UserStatus.readyToStart && _experienceRunning)
        {
            VideoFeed.instance.Dim(true);

            InstructionsTextBehavior.instance.ShowTextFromKey("otherIsGone");
            InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeOutImages();
            InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInPanel();

            instructionsTimeline.Stop();
            _experienceRunning = false;    
            StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
            selfStatus = UserStatus.headsetOn;
        }
        Debug.Log("the other user removed the headset", DLogType.Input);
    }

    public void Standby(bool start = false, bool dimOutOnExperienceStart = true, bool enablePresenceDetection = true)
    {
        if (!start) VideoFeed.instance.Dim(true); //TODO somehow this messes with Video Feed dimming when called on Start?
            InstructionsTextBehavior.instance.ShowTextFromKey("idle");

        instructionsTimeline.Stop();
        _experienceRunning = false;
        
        AudioManager.instance.StopAudioInstructions();

        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInPanel();
        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInText();
        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeOutImages();
        
        //reset user status as it is not ready
        EnableConfirmationGUI(true);
        _languageButtons.gameObject.SetActive(true); //show language buttons;
        LocalizationManager.instance.LoadLocalizedText(4);
        if (_readyForStandby) //TODO is check necessary? 
            ArduinoManager.instance.InitialPositions();
        
        Debug.Log("ready to start");
        
        if (!enablePresenceDetection)
        {
            VideoFeed.instance.Dim(true);
            InstructionsTextBehavior.instance.ShowInstructionText(false);
        }
        
        presenceDetection = enablePresenceDetection;

        _dimOutOnExperienceStart = dimOutOnExperienceStart;
        Debug.Log("setting dimOutOnExperienceStat to " + _dimOutOnExperienceStart);
    }

    public void EnablePresenceDetection(bool enablePresenceDetection)
    {
        
    }
    
    public void SerialFailure() //if something went wrong with the physical installation
    {
        VideoFeed.instance.Dim(true);
        OscManager.instance.SendSerialStatus(false);
        AudioManager.instance.StopAudioInstructions();    
        InstructionsTextBehavior.instance.ShowTextFromKey("systemFailure");
        instructionsTimeline.Stop();
        _experienceRunning = false;
        Destroy(gameObject);
        Debug.Log("serial failure", DLogType.Error);
    }

    public void SerialReady(bool serialControlComputer = false)
    {
        if (serialControlComputer) //if this computer is the one connected to the Arduino board
        {
            ArduinoManager.instance.InitialPositions();
        }
        
        InstructionsTextBehavior.instance.ShowTextFromKey("idle");
        _readyForStandby = true;
        Debug.Log("serial ready", DLogType.System);
    }    

    public void SelfRemovedHeadset()
    {
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (selfStatus == UserStatus.readyToStart) {
            Standby(false, _dimOutOnExperienceStart, true); //if we were ready and we took off the headset go to initial state
        }

        selfStatus = UserStatus.headsetOff;
        OscManager.instance.SendThisUserStatus(selfStatus);
        Debug.Log("this user removed his headset", DLogType.Input);
    }
    
    public void SetInstructionsTimeline(int index)
    {
        if (index == 0)
            instructionsTimeline = _shortTimeline;
        else if (index == 1)
            instructionsTimeline = _longTimeline;
    }

    
    #endregion


    #region Private Methods
    
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
            OpenCurtainCanvasController.instance.Show("Close Curtain");
            instructionsTimeline.Play();
            _experienceRunning = true;
        }
    }

    private void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.Dim(true);
        InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        instructionsTimeline.Stop();
		Debug.Log("experience finished");
        _experienceRunning = false;
    }

    private IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby(false, _dimOutOnExperienceStart, true); //if we were ready and we took off the headset go to initial state
        SelfPutHeadsetOn();
        Debug.Log("about to reset");
    }

    #endregion
}
 