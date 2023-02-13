using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recenter : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("n"))
        {
            UnityEngine.XR.InputTracking.Recenter();
            //The following will also move the camera positional reference.
            //taken from https://forum.unity.com/threads/openvr-how-to-reset-camera-properly.417509/#post-2792972
            //Valve.VR.OpenVR.System.ResetSeatedZeroPose();
            //Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
        }
    }
}
