using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class ConsentUIAutoBodySwapLogic : MonoBehaviour
{
    [SerializeField] private UserStateVariable _previousOtherState;

    public void SelfStateChanged(UserState selfState)
    {
        //if self is now ready to start 
        if (selfState == UserState.readyToStart) GetComponent<ConsentUI>().Show(true);
        else if (selfState == UserState.headsetOff) GetComponent<ConsentUI>().Show(false);
    }

    public void OtherStateChanged(UserState otherState)
    {
        if(otherState == UserState.headsetOff && _previousOtherState == UserState.readyToStart) 
            GetComponent<ConsentUI>().Show(true);
    }
}
