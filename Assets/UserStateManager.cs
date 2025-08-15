using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
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

}
