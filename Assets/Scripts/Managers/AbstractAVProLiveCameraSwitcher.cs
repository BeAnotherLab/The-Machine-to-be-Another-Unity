using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProLiveCamera;

public class AbstractAVProLiveCameraSwitcher : MonoBehaviour
{
    public void SetAVProCamera(int cameraIndex)
    {
        GetComponent<AVProLiveCamera>()._deviceSelection = AVProLiveCamera.SelectDeviceBy.Index;
        GetComponent<AVProLiveCamera>()._desiredDeviceIndex = cameraIndex;
        GetComponent<AVProLiveCamera>().Begin();
    }
}
