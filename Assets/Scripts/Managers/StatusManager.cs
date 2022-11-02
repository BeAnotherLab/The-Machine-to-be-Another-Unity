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
    
    [SerializeField] protected PlayableDirector _shortTimeline; //TODO shouldn't be in abstract status manager 
    [SerializeField] protected PlayableDirector _longTimeline; //TODO shouldn't be in abstract status manager
    [SerializeField] protected GameObject _languageButtons;

    [SerializeField] protected GameEvent _standbyGameEvent;
    [SerializeField] protected GameEvent _InstructionsStartedGameEvent;
    [SerializeField] protected BoolGameEvent _experienceFinishedGameEvent;
    [SerializeField] protected GameEvent _experienceStartedGameEvent;
    [SerializeField] protected StringGameEvent _languageChangeEvent;
    [SerializeField] protected BoolGameEvent _curtainOnEvent;
    
    [SerializeField] protected StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] protected BoolGameEvent _showInstructionsTextGameEvent;

    [SerializeField] protected TrackAsset _germanTrack;
    [SerializeField] protected TrackAsset _englishTrack;
    
    protected GameObject _confirmationMenu; //TODO use events, no direct reference!
    protected bool _experienceRunning;
    protected bool _dimOutOnExperienceStart;
    
    protected delegate void OnShowTimedText(string key, int time);
    protected OnShowTimedText ShowTimedText;
    
    #endregion

    #region Monobehaviour Methods
    
    protected void Awake()
    {
        if (instance == null) instance = this;

        _confirmationMenu = GameObject.Find("ConfirmationMenu"); //TOOD don't use references like that
        UduinoManager.Instance.OnBoardDisconnectedEvent.AddListener(delegate {
            //SerialFailure(); //TODO wait for a few seconds for reconnection instead of going staight to failure
        });
        instructionsTimeline = _longTimeline; //use short experience by default
    }

    // Start is called before the first frame update
    protected void Start()
    {
        if (SwapModeManager.instance.ArduinoControl)
            _setInstructionsTextGameEvent.Raise("serial");

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
        _showInstructionsTextGameEvent.Raise(false);
        if (_dimOutOnExperienceStart) VideoFeed.instance.Dim(false);
        else instructionsTimeline.Stop();
        _experienceStartedGameEvent.Raise();
        Debug.Log("experience started");
    }
    
    public void SerialFailure() //if something went wrong with the physical installation
    {
        VideoFeed.instance.Dim(true);
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
        Debug.Log("wall on");        
        _curtainOnEvent.Raise(true);
    }
    
    public void WallOn() //TODO rename
    {
        _curtainOnEvent.Raise(false);
        //ArduinoManager.instance.SendCommand("mir_off"); //hide mirror
        Debug.Log("wall off");
    }

    protected void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        OscManager.instance.SendThisUserStatus(UserState.readyToStart);
        _languageButtons.gameObject.SetActive(false); //hide language buttons;
        _setInstructionsTextGameEvent.Raise("waitForOther");
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
        if (!start) VideoFeed.instance.Dim(true); //TODO somehow this messes with Video Feed dimming when called on Start?
        _setInstructionsTextGameEvent.Raise("idle");

        instructionsTimeline.Stop();
        _experienceRunning = false;
        
        AudioManager.instance.StopAudioInstructions();

        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeInText(); //TODO use events instead of static reference
        InstructionsTextBehavior.instance.gameObject.GetComponent<FadeController>().FadeOutImages();  //TODO use events instead of static reference
        
        _languageButtons.gameObject.SetActive(true); //show language buttons;

        Debug.Log("ready to start");
        
        VideoFeed.instance.Dim(true); //TODO use events instead of static reference

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
        instructionsTimeline.Play();
        _InstructionsStartedGameEvent.Raise();
        _experienceRunning = true;
    }

    #endregion
    
}
