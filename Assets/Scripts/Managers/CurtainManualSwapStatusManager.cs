using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = DebugFile;

public class CurtainManualSwapStatusManager : StatusManager
{

    public new void SelfStateChanged(UserState newState) //TODO move to own state changes events class
    {
        if (newState == UserState.headsetOff) SelfRemovedHeadset();
        else if (newState == UserState.headsetOn) SelfPutHeadsetOn();
        else if (newState == UserState.readyToStart)
        {
            OscManager.instance.SendThisUserStatus((UserState.readyToStart));
            _languageButtons.gameObject.SetActive(false); //hide language buttons;

            if (otherState.Value == UserState.readyToStart) StartPlaying(); //TODO this should be the default behavior
        
            _setInstructionsTextGameEvent.Raise("waitForOther");
            Debug.Log("this user is ready", DLogType.Input);
        }
    }

    public new void OtherStateChanged(UserState newState)
    {
        if (newState == UserState.headsetOff) OtherLeft();
        else if (newState == UserState.headsetOn) OtherPutHeadsetOn(); //TODO only if previous one was ready to start?
        else if (newState == UserState.readyToStart) OtherUserIsReady();
    }
    
    public void OtherUserIsReady()
    {
        Debug.Log("the other user is ready", DLogType.Input);
        if (selfState.Value == UserState.readyToStart) StartPlaying();//TODO this should be the default behavior
    }

    
    private void Start()    
    {
        _standbyGameEvent.Raise();
    }
}
