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

            if (otherState.Value == UserState.readyToStart) StartPlaying();
        
            _setInstructionsTextGameEvent.Raise("waitForOther");
            Debug.Log("this user is ready", DLogType.Input);
        }
    }

    public new void OtherStateChanged(UserState newState) //TODO move to own state changes events class
    {
         base.OtherStateChanged(newState);
    }

}
