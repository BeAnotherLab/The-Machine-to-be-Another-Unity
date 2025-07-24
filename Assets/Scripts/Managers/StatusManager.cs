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
    public static OnStopAllAudios StopAudiosInstructions = delegate { };

    public delegate void OnSendThisUserStatus(UserState state);
    public static OnSendThisUserStatus SendThisUserStatus; //TODO this needs not to be here, OSCManager can send all changes by itself
    public delegate void OnSendArduinoCommand(string command);
    public static OnSendArduinoCommand SendArduinoCommand;
    
    #endregion
    
    #region Protected Fields

    [SerializeField] private BoolGameEvent _dimGameEvent;
    [SerializeField] protected PlayableDirector _shortTimeline;  
    [SerializeField] protected PlayableDirector _longTimeline; 
    [SerializeField] protected GameObject _languageButtons; //TODO use events, no direct references

    [SerializeField] protected GameEvent _standbyGameEvent;
    [SerializeField] protected GameEvent _InstructionsStartedGameEvent;
    [SerializeField] protected BoolGameEvent _experienceFinishedGameEvent; //TODO why bool?
    [SerializeField] protected GameEvent _experienceStartedGameEvent;
    [SerializeField] protected BoolGameEvent _curtainOnEvent;
    
    [SerializeField] protected StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] protected BoolGameEvent _showInstructionsTextGameEvent;

    [SerializeField] protected TrackAsset _germanTrack;
    [SerializeField] protected TrackAsset _englishTrack;
    
    protected bool _experienceRunning;
    
    #endregion

    #region Monobehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialFailure += SerialFailure;
        ArduinoManager.SerialReady += Standby;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialFailure -= SerialFailure;
        ArduinoManager.SerialReady -= Standby;
    }

    private void Awake()
    {
        instructionsTimeline = _longTimeline; //TODO remove?
    }

    protected void Start()
    {
        _setInstructionsTextGameEvent.Raise("waitForSerial"); 
        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    protected void Update()
    {
        if (SessionStateFeature.GetCurrentState() == (int) XrSessionState.Idle  && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; 
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
        //TODO this might not work with all headsets 
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
        Debug.Log("experience started", DLogType.Logic);
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
        Debug.Log("mirrors on", DLogType.Logic);
    }

    public void CloseWall()
    {
        Debug.Log("wall on", DLogType.Logic);        
        _curtainOnEvent.Raise(true);
    }
    
    public void WallOn() //TODO rename to Wall off
    {
        _curtainOnEvent.Raise(false);
        SendArduinoCommand("mir_off"); //hide mirror
        Debug.Log("wall off", DLogType.Logic);
    }

    protected void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        SendThisUserStatus(UserState.readyToStart); //TODO this needs not to be here, OSCManager can send all changes by itself
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
        SendThisUserStatus(UserState.headsetOn); //TODO this needs not to be here, OSCManager can send all changes by itself
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
    
    public void Standby()
    {
        Debug.Log("Standby");
        instructionsTimeline.Stop();
        _setInstructionsTextGameEvent.Raise("idle");
        _experienceRunning = false;
        StopAudiosInstructions();
        _languageButtons.gameObject.SetActive(true); //show language buttons;
        Debug.Log("ready to start", DLogType.Logic); //TODO why ready to start and standby at same time?
        _dimGameEvent.Raise(true);
        _standbyGameEvent.Raise();
    }

    public void SelfRemovedHeadset()
    {
       if (previousSelfState.Value == UserState.readyToStart) Standby(); //if we were ready and we took off the headset go to initial state
        SendThisUserStatus(selfState); ////TODO this needs not to be here, OSCManager can send all changes by itself
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
        Debug.Log("experience finished", DLogType.Logic);
        _experienceRunning = false;
        _experienceFinishedGameEvent.Raise(false);
    }
    
    protected IEnumerator WaitBeforeResetting()
    {
        Debug.Log("about to reset", DLogType.Logic);
        yield return new WaitForSeconds(4f); //make sure this value is inferior or equal to the confirmation radial time to avoid bugs
        Standby(); //if we were ready and we took off the headset go to initial state
        SelfPutHeadsetOn();
    }

    protected void StartPlaying()
    {
        instructionsTimeline.Play();
        _InstructionsStartedGameEvent.Raise();
        _experienceRunning = true;
    }

    #endregion
    
}
