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
using UnityEngine.XR.OpenXR.NativeTypes;
using VRStandardAssets.Utils;

public class StatusManager : MonoBehaviour
{
    #region Public Fields

    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    
    public UserStateGameEvent selfStateGameEvent;
    public UserStateGameEvent otherStateGameEvent;
    
    public PlayableDirector instructionsTimeline;

    public delegate void OnStopAllAudios();
    public static OnStopAllAudios StopAudiosInstructions;

    public delegate void OnSendThisUserStatus(UserState state);
    public static OnSendThisUserStatus SendThisUserStatus;
    
    public delegate void OnSendArduinoCommand(string command);
    public static OnSendArduinoCommand SendArduinoCommand;
    
    public delegate void OnInitializeInstructions();
    public static OnInitializeInstructions InitializeInstructions;

    
    #endregion
    
    #region Protected Fields

    [SerializeField] private BoolGameEvent _dimGameEvent;
    [SerializeField] protected PlayableDirector _shortTimeline; //TODO shouldn't be in abstract status manager 
    [SerializeField] protected PlayableDirector _longTimeline; //TODO shouldn't be in abstract status manager
    [SerializeField] protected GameObject _languageButtons; //TODO use events

    [SerializeField] protected GameEvent _standbyGameEvent;
    [SerializeField] protected GameEvent _InstructionsStartedGameEvent;
    [SerializeField] protected BoolGameEvent _experienceFinishedGameEvent; //TODO why bool?
    [SerializeField] protected GameEvent _experienceStartedGameEvent;
    [SerializeField] protected BoolGameEvent _curtainOnEvent;
    
    [SerializeField] protected StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] protected BoolGameEvent _showInstructionsTextGameEvent;

    [SerializeField] protected TrackAsset _germanTrack;
    [SerializeField] protected TrackAsset _englishTrack;
    
    protected GameObject _confirmationMenu; //TODO use events, no direct reference!
    protected bool _experienceRunning;
    protected bool _dimOutOnExperienceStart;
    
    #endregion

    #region Monobehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialFailure += SerialFailure;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialFailure -= SerialFailure;
    }

    private void Awake()
    {
        _confirmationMenu = GameObject.Find("ConfirmationMenu"); //TOOD don't use references like that
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate {
            SerialFailure(); //TODO wait for a few seconds for reconnection instead of going staight to failure
        });
        instructionsTimeline = _longTimeline; //use short experience by default
    }

    protected void Start()
    {
        _setInstructionsTextGameEvent.Raise("waitForSerserial"); 
        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    protected void Update()
    {
        if (SessionStateFeature.GetCurrentState() == (int) XrSessionState.Synchronized  && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; 
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
        else if (SessionStateFeature.GetCurrentState() == (int) XrSessionState.Focused && selfState.Value == UserState.headsetOff) //if we just put the headset on
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
        _showInstructionsTextGameEvent.Raise(false);
        _dimGameEvent.Raise(false);
        _experienceStartedGameEvent.Raise();
        Debug.Log("experience started");
    }
    
    public void SerialFailure() //if something went wrong with the physical installation
    {
        _dimGameEvent.Raise(true);
        StopAudiosInstructions(); 
        _setInstructionsTextGameEvent.Raise("systemFailure");
        instructionsTimeline.Stop();
        _experienceRunning = false;
        Destroy(gameObject);
        Debug.Log("serial failure", DLogType.Error);
    }

    public void MirrorOn()
    {
        SendArduinoCommand("mir_on"); 
        Debug.Log("mirrors on");
    }

    public void CloseWall()
    {
        Debug.Log("wall on");        
        _curtainOnEvent.Raise(true);
    }
    
    public void WallOn() //TODO rename to Wall off
    {
        _curtainOnEvent.Raise(false);
        SendArduinoCommand("mir_off"); //hide mirror
        Debug.Log("wall off");
    }

    protected void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        SendThisUserStatus(UserState.readyToStart);
        _languageButtons.gameObject.SetActive(false); //hide language buttons;
        if (otherState.Value == UserState.readyToStart) StartPlaying(); //TODO this should be the default behavior
        _setInstructionsTextGameEvent.Raise("waitForOther");
        Debug.Log("this user is ready", DLogType.Input);
    }

    public void OtherUserIsReady()
    {
        Debug.Log("the other user is ready", DLogType.Input);
        if (selfState.Value == UserState.readyToStart) StartPlaying();//TODO this should be the default behavior
    }

    public void SelfPutHeadsetOn()
    {
        _setInstructionsTextGameEvent.Raise("idle");
        SendThisUserStatus(UserState.headsetOn);
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
                _setInstructionsTextGameEvent.Raise("otherIsGone");
                StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
                selfState.Value = UserState.headsetOn;    
            }
        }
        Debug.Log("the other user removed the headset", DLogType.Input);
    }
    
    public void Standby(bool start = false, bool dimOutOnExperienceStart = true)
    {
        Debug.Log("Standby");
        if (!start) _dimGameEvent.Raise(true);; //TODO somehow this messes with Video Feed dimming when called on Start?
        _setInstructionsTextGameEvent.Raise("idle");

        instructionsTimeline.Stop();
        _experienceRunning = false;

        StopAudiosInstructions();

        InitializeInstructions();
        
        _languageButtons.gameObject.SetActive(true); //show language buttons;

        Debug.Log("ready to start");
        
        _dimGameEvent.Raise(true); //TODO this is called twice?

        _dimOutOnExperienceStart = dimOutOnExperienceStart;
        Debug.Log("setting dimOutOnExperienceStat to " + _dimOutOnExperienceStart);
        
        _standbyGameEvent.Raise();
    }

    public void SelfRemovedHeadset()
    {
        //TODO use event instead 
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (previousSelfState.Value == UserState.readyToStart) {
            Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        }
        
        SendThisUserStatus(selfState); //TODO use events instead
        Debug.Log("this user removed his headset", DLogType.Input);
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
        _dimGameEvent.Raise(true);
        _setInstructionsTextGameEvent.Raise("finished");
        instructionsTimeline.Stop();
        Debug.Log("experience finished");
        _experienceRunning = false;
        _experienceFinishedGameEvent.Raise(false);
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
        instructionsTimeline.Play();
        _InstructionsStartedGameEvent.Raise();
        _experienceRunning = true;
    }

    #endregion
    
}
