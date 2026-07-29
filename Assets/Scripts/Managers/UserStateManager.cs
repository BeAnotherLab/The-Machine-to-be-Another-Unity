using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = DebugFile;
using UnityEngine.XR.OpenXR.NativeTypes;

//Manual Swap User State Manager
public class UserStateManager : MonoBehaviour //TODO make options for Vive/Rift switch with or without area detection
{
    //Those are kept public so they can be accessed from the editor script and triggered with inspector buttons
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
    
    public delegate void OnOtherLeft();
    public static OnOtherLeft OtherLeft = delegate { };

    public delegate void OnThisUserLeft();
    public static OnThisUserLeft ThisUserLeft = delegate { };
    
    [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;
    [SerializeField] private BoolVariable _experienceRunning;

    private void Start()
    {
        selfState.Value = UserState.headsetOff;
        otherState.Value = UserState.headsetOff;
    }

    private void Update() //Monitor VR headset state changes to infer user presence
    {
        //TODO this will not work with all headsets. this is for Rift CV1 only
        if (!OVRPlugin.userPresent && selfState.Value != UserState.headsetOff)
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOff; 
            selfStateGameEvent.Raise(UserState.headsetOff);
        }
       
        else if (OVRPlugin.userPresent && selfState.Value == UserState.headsetOff) //if we just put the headset on
        {
            previousSelfState.Value = selfState.Value;
            selfState.Value = UserState.headsetOn;
            selfStateGameEvent.Raise(UserState.headsetOn);
        }
    }
    
    public void SelfStateChanged(UserState newState) //this can be triggered by headset of confirmation button
    {
        if (newState == UserState.headsetOff)
        {
            Debug.Log("this user removed his headset", DLogType.Input);
            if (previousSelfState.Value == UserState.readyToStart) {
                ThisUserLeft(); //if we were ready and we took off the headset go to initial state
                _experienceRunning.Value = false;
            }
        }
        else if (newState == UserState.headsetOn)
        {
            _setInstructionsTextGameEvent.Raise("idle"); //TODO self manage
            Debug.Log("this user put on the headset", DLogType.Input);
        }
        else if (newState == UserState.readyToStart)
        {
            if (otherState.Value == UserState.readyToStart)
            {
                BothUsersReady();
                _experienceRunning.Value = true;
            }
            _setInstructionsTextGameEvent.Raise("waitForOther"); //TODO self manage
            Debug.Log("this user is ready", DLogType.Input);
        }
        
        SendThisUserStatus(newState); 
    }

    public void OtherStateChanged(UserState newState)
    {
        if (newState == UserState.headsetOff)
        {
            if (previousOtherState.Value == UserState.readyToStart && _experienceRunning.Value) // if the other removed the headset during the experience and 
            {
                OtherLeft();
                _experienceRunning.Value = false;
                StartCoroutine(WaitAndSetHeadsetOn());
                _setInstructionsTextGameEvent.Raise("otherIsGone");    
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
            if (selfState.Value == UserState.readyToStart) {
                BothUsersReady();//TODO this should be the default behavior
                _experienceRunning.Value = true;
            }       
        }
    }

    private IEnumerator WaitAndSetHeadsetOn()
    {
        yield return new WaitForSeconds(4f);
        SelfStateChanged(UserState.headsetOn);
    }

    public void Standby() //TODO is this redundant 
    {
        selfState.Value = UserState.headsetOn;
       // SelfStateChanged(UserState.headsetOn);
    }

}
