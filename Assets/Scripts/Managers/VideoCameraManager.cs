using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using RockVR.Video;
using UnityEngine;

public class VideoCameraManager : MonoBehaviour
{
    public delegate void WebCamConnected();
    public event WebCamConnected OnWebCamConnected;
    private int _connectedWebcams;

    public static VideoCameraManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_connectedWebcams != AVProLiveCameraManager.Instance.NumDevices)
        {
            _connectedWebcams = AVProLiveCameraManager.Instance.NumDevices;
            OnWebCamConnected();
        }
    }

    public void SetAVProCamera(int cameraIndex)
    {
        GetComponent<AVProLiveCamera>()._deviceSelection = AVProLiveCamera.SelectDeviceBy.Index;
        GetComponent<AVProLiveCamera>()._desiredDeviceIndex = cameraIndex;
        GetComponent<AVProLiveCamera>().Begin();
    }
}
