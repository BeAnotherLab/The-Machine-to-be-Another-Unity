using System;
using UnityEngine;
using VRStandardAssets.Utils;

public class AutoBodySwapConfirmationButton : MonoBehaviour //TODO inherit confirmation button?
{
    private void Start()
    {
        Show(false);
    }

    private void OnEnable()
    {
        ArduinoManager.SerialFailure += SerialFailure;
        ArduinoManager.SerialReady += SerialReady;
    }

    private void OnDisable()
    {
        ArduinoManager.SerialFailure -= SerialFailure;
        ArduinoManager.SerialReady -= SerialReady;
    }

    
    public void SelfUserStateChanged(UserState selfUserState)
    {
        if (selfUserState == UserState.readyToStart)
        {
            Show(false);    
        }
        if (selfUserState == UserState.headsetOff)
        {
            GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more            
        }
    }

    public void SerialFailure()
    {
        Show(false);
    }

    public void SerialReady()
    {
        Show(true);
    }

    private void Show(bool show)
    {
        GetComponent<MeshRenderer>().enabled = show;
        GetComponent<MeshCollider>().enabled = show;
    }
}
