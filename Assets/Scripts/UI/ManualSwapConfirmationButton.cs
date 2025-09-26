using System;
using UnityEngine;
using VRStandardAssets.Utils;

public class ManualBodySwapConfirmationButton : MonoBehaviour //TODO inherit confirmation button?
{
    private void Start()
    {
        Show(true);
    }

    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart) Show(false);
        if (selfUserState == UserState.headsetOff) GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more            
    }
    
    private void Show(bool show)
    {
        GetComponent<MeshRenderer>().enabled = show;
        GetComponent<MeshCollider>().enabled = show;
    }
}
