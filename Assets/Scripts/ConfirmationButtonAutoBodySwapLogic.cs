using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using VRStandardAssets.Menu;

public class ConfirmationButtonAutoBodySwapLogic : MonoBehaviour //TODO must make one different for Manual Swap?
{
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
}
