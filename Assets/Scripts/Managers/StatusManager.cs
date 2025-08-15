using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Debug = DebugFile;
using UnityEngine.XR.OpenXR.NativeTypes;

public class StatusManager : MonoBehaviour //TODO instructions text stuff needs not be handled here
{
    #region Public Fields
    
    //Those are kept public so they can be accessed from the editor script and triggered with buttons
    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    
    public delegate void OnStopSequencer();
    public static OnStopSequencer StopSequencer = delegate { };
    
    public delegate void OnStartSequencer();
    public static OnStartSequencer StartSequencer = delegate { };
    
    public delegate void OnStopAllAudios();
    public static OnStopAllAudios StopAudiosInstructions = delegate { };

    public delegate void OnSendThisUserStatus(UserState state);
    public static OnSendThisUserStatus SendThisUserStatus; //TODO this needs not to be here, OSCManager can send all changes by itself
    public delegate void OnSendArduinoCommand(string command);
    public static OnSendArduinoCommand SendArduinoCommand;
    
    #endregion
    
    //using protected to make them accessible to children 
    #region Protected Fields 
    [SerializeField] protected BoolGameEvent _dimGameEvent;

    [SerializeField] protected GameEvent _standbyGameEvent;
    [SerializeField] protected GameEvent _InstructionsStartedGameEvent;
    [SerializeField] protected BoolGameEvent _experienceFinishedGameEvent; //TODO why bool?
    [SerializeField] protected GameEvent _experienceStartedGameEvent;
    [SerializeField] protected BoolGameEvent _curtainOnEvent;
    
    [SerializeField] protected StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] protected BoolGameEvent _showInstructionsTextGameEvent;   
    
    protected bool _experienceRunning;
    
    #endregion

    #region Monobehaviour Methods

    private void OnEnable()
    {
        ArduinoManager.SerialFailure += SerialFailure;
        ArduinoManager.SerialReady += Standby;
        OscManager.ReceiveSerialFailure += SerialFailure;
        OscManager.ReceiveSerialReady += Standby;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialFailure -= SerialFailure;
        ArduinoManager.SerialReady -= Standby;
        OscManager.ReceiveSerialFailure -= SerialFailure;
        OscManager.ReceiveSerialReady -= Standby;
    }

    protected void Start()
    {
        _setInstructionsTextGameEvent.Raise("waitForSerial"); 
    }

    protected void Update()
    {
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
    
    private void SerialFailure() //if something went wrong with the physical installation
    {
        _dimGameEvent.Raise(true);
        StopAudiosInstructions(); 
        _setInstructionsTextGameEvent.Raise("systemFailure");
        StopSequencer();
        _experienceRunning = false;
        Destroy(gameObject);
        Debug.Log("serial failure", DLogType.Error);
    }

    public void MirrorOn()
    {
        SendArduinoCommand("mir_on"); 
        Debug.Log("mirrors on", DLogType.Logic);
    }

    public void CloseWall() //called 
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

    private void ThisUserIsReady() //called when user has aimed at the confirmation dialog and waited through the countdown.
    {
        SendThisUserStatus(UserState.readyToStart); //TODO this needs not to be here, OSCManager can send all changes by itself
        if (otherState.Value == UserState.readyToStart) StartPlaying(); //TODO this should be the default behavior
        _setInstructionsTextGameEvent.Raise("waitForOther"); //TODO self manage
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
            //only reset on other left if experience running
            if (_experienceRunning)
            {
                StopSequencer();
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
        StopSequencer();
        _setInstructionsTextGameEvent.Raise("idle");
        _experienceRunning = false;
        StopAudiosInstructions();
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
    
    #endregion
    
    #region Protected Methods

    protected void IsOver() //called at the the end of the experience
    {
        _dimGameEvent.Raise(true);
        _setInstructionsTextGameEvent.Raise("finished");
        StopSequencer();
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
        StartSequencer();
        _InstructionsStartedGameEvent.Raise();
        _experienceRunning = true;
    }

    #endregion
    
}
