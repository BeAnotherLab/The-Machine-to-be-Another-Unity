using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = DebugFile;
using UnityEngine.XR.OpenXR.NativeTypes;

public class UserStateManager : MonoBehaviour
{
    //Those are kept public so they can be accessed from the editor script and triggered with buttons
    public UserStateVariable previousOtherState;
    public UserStateVariable otherState;
    
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    
    public UserStateGameEvent selfStateGameEvent;
    public UserStateGameEvent otherStateGameEvent;

    
    public delegate void OnSendThisUserStatus(UserState state);
    public static OnSendThisUserStatus SendThisUserStatus;

    public delegate void OnBothUsersReady();
    public static OnBothUsersReady BothUsersReady;
    
    public delegate void OnStopSequencer();
    public static OnStopSequencer StopSequencer = delegate { };

    [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] private BoolVariable _experienceRunning;

    private void Start()
    {
        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    private void Update()
    {
        if (SessionStateFeature.GetCurrentState() == (int) XrSessionState.Synchronized  && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; 
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
        //TODO this will not work with all headsets 
        else if (SessionStateFeature.GetCurrentState() == (int) XrSessionState.Focused && selfState.Value == UserState.headsetOff) //if we just put the headset on
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOn;
            selfStateGameEvent.Raise(UserState.headsetOn);
        }
    }
    
    public void SelfStateChanged(UserState newState) 
    {
        if (newState == UserState.headsetOff)
        {
            if (previousSelfState.Value == UserState.readyToStart) Standby(); //if we were ready and we took off the headset go to initial state
            SendThisUserStatus(selfState); ////TODO this needs not to be here, OSCManager can send all changes by itself
            Debug.Log("this user removed his headset", DLogType.Input);
        }
        else if (newState == UserState.headsetOn)
        {
            SelfPutHeadsetOn();
        }
        else if (newState == UserState.readyToStart)
        {
            SendThisUserStatus(UserState.readyToStart); //TODO this needs not to be here, OSCManager can send all changes by itself
            if (otherState.Value == UserState.readyToStart) BothUsersReady(); //TODO this should be the default behavior
            _setInstructionsTextGameEvent.Raise("waitForOther"); //TODO self manage
            Debug.Log("this user is ready", DLogType.Input);
        }
    }

    public void OtherStateChanged(UserState newState) //TODO move to own state changes events class
    {
        if (newState == UserState.headsetOff)
        {
            if (previousOtherState.Value == UserState.readyToStart) //TODO remove?
            {
                if (_experienceRunning.Value) //only reset on other left if experience running
                {
                    StopSequencer();
                    _setInstructionsTextGameEvent.Raise("otherIsGone");
                    StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
                    selfState.Value = UserState.headsetOn; //no longer ready    
                }
            }
            Debug.Log("the other user removed the headset", DLogType.Input);
        }
        else if (newState == UserState.headsetOn)
        {
            Debug.Log("the other user put on the headset", DLogType.Input);
        }
        else if (newState == UserState.readyToStart)
        {
            Debug.Log("the other user is ready", DLogType.Input);
            if (selfState.Value == UserState.readyToStart) BothUsersReady();//TODO this should be the default behavior
        }
    }

    private void SelfPutHeadsetOn()
    {
        _setInstructionsTextGameEvent.Raise("idle");
        SendThisUserStatus(UserState.headsetOn); //TODO this needs not to be here, OSCManager can send all changes by itself
        Debug.Log("this user put on the headset", DLogType.Input);
    }
}
