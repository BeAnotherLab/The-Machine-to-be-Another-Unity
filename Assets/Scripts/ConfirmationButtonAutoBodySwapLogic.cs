using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using VRStandardAssets.Menu;

public class ConfirmationButtonAutoBodySwapLogic : MonoBehaviour
{
    [SerializeField] private UserStateGameEvent selfStateGameEvent;
    [SerializeField] private UserStateVariable selfState;
    
    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshCollider>().enabled = false;    
        }
    }
    
    public void OnStandby()
    {
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<MeshCollider>().enabled = true;
    }

    public void HandleSelectionComplete()
    {
        if (GetComponent<ConfirmationButton>())
        {
            selfState.Value = UserState.readyToStart;
            selfStateGameEvent.Raise(selfState.Value);    
        }
    }
}
