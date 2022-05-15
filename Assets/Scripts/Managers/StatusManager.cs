using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using Uduino;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Debug = DebugFile;
using UnityEngine.XR;
using VRStandardAssets.Utils;

public abstract class StatusManager : MonoBehaviour
{
    #region Public Fields

    public static StatusManager instance; //TODO remove

    public bool presenceDetection; //TODD check if still necessary
    
    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    
    public UserStateGameEvent selfStateGameEvent;
    public UserStateGameEvent otherStateGameEvent;
    
    public PlayableDirector instructionsTimeline;
    
    #endregion
    
    #region Protected Fields
    
    [SerializeField] protected PlayableDirector _shortTimeline;
    [SerializeField] protected PlayableDirector _longTimeline;
    [SerializeField] protected GameObject _languageButtons;

    [SerializeField] protected GameEvent _standbyGameEvent;
    [SerializeField] protected GameEvent _InstructionsStartedGameEvent;
    [SerializeField] protected BoolGameEvent _experienceFinishedGameEvent;
    [SerializeField] protected GameEvent _experienceStartedGameEvent;
    [SerializeField] protected StringGameEvent _languageChangeEvent;
    [SerializeField] protected BoolGameEvent _curtainOnEvent;
    
    [SerializeField] protected StringGameEvent _setInstructionsTextGameEvent;

    [SerializeField] protected TrackAsset _germanTrack;
    [SerializeField] protected TrackAsset _englishTrack;
    
    protected bool _readyForStandby; //when we use serial, only go to standby if Arduino is ready.
    protected GameObject _confirmationMenu; //TODO use events, no direct reference!
    protected bool _experienceRunning;
    protected bool _dimOutOnExperienceStart;
    
    #endregion

    #region Monobehaviour Methods
    
    protected void Awake()
    {
        if (instance == null) instance = this;

        _confirmationMenu = GameObject.Find("ConfirmationMenu");
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate { SerialFailure(); });
        instructionsTimeline = _longTimeline; //use short experience by default
    }

    // Start is called before the first frame update
    protected void Start()
    {
        if (SwapModeManager.instance.ArduinoControl)
            _setInstructionsTextGameEvent.Raise("serial");
        else
            _readyForStandby = true; //if we're not using the serial control, we don't have to wait for the arduino

        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (XRDevice.userPresence == UserPresenceState.NotPresent && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; //SelfRemovedHeadset();
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
        else if (XRDevice.userPresence == UserPresenceState.Present && selfState.Value == UserState.headsetOff) //if we just put the headset on
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOn;
            selfStateGameEvent.Raise(UserState.headsetOn);
        }
            
        if (Input.GetKeyDown("o")) IsOver();
    }
    
    #endregion
    
    #region Public Methods 
    
    public void StartExperience() //TODO remove?
    {
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        if (_dimOutOnExperienceStart) VideoFeed.instance.Dim(false);
        else instructionsTimeline.Stop();
        _experienceStartedGameEvent.Raise();
        Debug.Log("experience started");
    }
    
    public void SerialFailure() //if something went wrong with the physical installation
    {
        VideoFeed.instance.Dim(true);
        OscManager.instance.SendSerialStatus(false);
        AudioManager.instance.StopAudioInstructions();    
        _setInstructionsTextGameEvent.Raise("systemFailure");
        instructionsTimeline.Stop();
        _experienceRunning = false;
        Destroy(gameObject);
        Debug.Log("serial failure", DLogType.Error);
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
        _curtainOnEvent.Raise(true);
    }
    
    public void WallOn() //TODO rename
    {
        OpenCurtainCanvasController.instance.Show("Open Curtain");
        ArduinoManager.instance.WallOn(false);
        _curtainOnEvent.Raise(false);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall off");
    }

    public void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        OscManager.instance.SendThisUserStatus(UserState.readyToStart);
        _languageButtons.gameObject.SetActive(false); //hide language buttons;
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        Debug.Log("this user is ready", DLogType.Input);
    }

    public void OtherUserIsReady()
    {
        Debug.Log("the other user is ready", DLogType.Input);
    }

    public void SelfPutHeadsetOn()
    {
        _setInstructionsTextGameEvent.Raise("idle");
        OscManager.instance.SendThisUserStatus(UserState.headsetOn);
        Debug.Log("this user put on the headset", DLogType.Input);
    }

    public void OtherPutHeadsetOn()
    {
        Debug.Log("the other user put on the headset", DLogType.Input);
    }

    public void OtherLeft()
    {
        //if experience started
        if (previousOtherState.Value == UserState.readyToStart)
        {
            //only reset on other left if experience running, post finished, or doing pre questionnaire
            if (_experienceRunning) 
            {
                instructionsTimeline.Stop();
                _experienceRunning = false;    
                StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
                selfState.Value = UserState.headsetOn;    
            }
        }
        Debug.Log("the other user removed the headset", DLogType.Input);
    }
    
    public void Standby(bool start = false, bool dimOutOnExperienceStart = true)
    {
        if (!start) VideoFeed.instance.Dim(true); //TODO somehow this messes with Video Feed dimming when called on Start?
        _setInstructionsTextGameEvent.Raise("idle");

        instructionsTimeline.Stop();
        _experienceRunning = false;
        
        AudioManager.instance.StopAudioInstructions();

        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInText(); //TODO use events instead of static reference
        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeOutImages();  //TODO use events instead of static reference
        
        //reset user status as it is not ready
        _languageChangeEvent.Raise("German");  //reset to German by default
        _languageButtons.gameObject.SetActive(true); //show language buttons;

        if (_readyForStandby) //TODO is check necessary? 
            ArduinoManager.instance.InitialPositions();
        
        Debug.Log("ready to start");
        
        VideoFeed.instance.Dim(true); //TODO use events instead of static reference

        _dimOutOnExperienceStart = dimOutOnExperienceStart;
        Debug.Log("setting dimOutOnExperienceStat to " + _dimOutOnExperienceStart);
        
        _standbyGameEvent.Raise();
    }

    public void SerialReady(bool serialControlComputer = false)
    {
        if (serialControlComputer) //if this computer is the one connected to the Arduino board
        {
            ArduinoManager.instance.InitialPositions();
        }
        
        _setInstructionsTextGameEvent.Raise("idle");
        _readyForStandby = true;
        Debug.Log("serial ready", DLogType.System);
    }    
    
    public void SelfRemovedHeadset()
    {
        //TODO use event instead 
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (previousSelfState.Value == UserState.readyToStart) {
            Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        }
        
        OscManager.instance.SendThisUserStatus(selfState); //TODO use events instead
        Debug.Log("this user removed his headset", DLogType.Input);
    }

    public void SetInstructionsTimeline(int index) //TODO remove?
    {
        if (index == 0)
            instructionsTimeline = _shortTimeline;
        else if (index == 1)
            instructionsTimeline = _longTimeline;
    }

    public void SelfStateChanged(UserState newState) //TODO move to own state changes events class
    {
        if (newState == UserState.headsetOff) SelfRemovedHeadset();
        else if (newState == UserState.headsetOn) SelfPutHeadsetOn();
        else if (newState == UserState.readyToStart) ThisUserIsReady();
    }

    public void OtherStateChanged(UserState newState) //TODO move to own state changes events class
    {
        if (newState == UserState.headsetOff) OtherLeft();
        else if (newState == UserState.headsetOn) OtherPutHeadsetOn(); //TODO only if previous one was ready to start?
        else if (newState == UserState.readyToStart) OtherUserIsReady();
    }
    
    public void SwitchLanguageTrack(string language)
    {
        TimelineAsset timelineAsset = (TimelineAsset) instructionsTimeline.playableAsset;
        _englishTrack = timelineAsset.GetOutputTrack(0);
        _germanTrack = timelineAsset.GetOutputTrack(1);
        _englishTrack.muted = language != "English";
        _germanTrack.muted = language != "German";
    }

    
    #endregion
    
    #region Protected Methods

    protected void IsOver() //called at the the end of the experience
    {
        VideoFeed.instance.Dim(true);
        //InstructionsTextBehavior.instance.ShowTextFromKey("finished");
        instructionsTimeline.Stop();
        Debug.Log("experience finished");
        _experienceRunning = false;
    }
    
    protected IEnumerator WaitBeforeResetting()
    {
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        SelfPutHeadsetOn();
        Debug.Log("about to reset");
    }

    protected void StartPlaying()
    {
        if (_readyForStandby)
        {
            OpenCurtainCanvasController.instance.Show("Close Curtain");
            instructionsTimeline.Play();
            _InstructionsStartedGameEvent.Raise();
            _experienceRunning = true;
        }
    }

    #endregion
    
}
