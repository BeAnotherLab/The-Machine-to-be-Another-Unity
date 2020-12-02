using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using RenderHeads.Media.AVProLiveCamera.Demos;
using RockVR.Video;
using Unity.VideoHelper;
using UnityEngine;
using UnityEngine.UI;

public class VideoCameraManager : AbstractAVProLiveCameraSwitcher
{
    public static VideoCameraManager instance;

    private VideoController _videoController;

    private AVProLiveCamera _avProLiveCamera;
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _videoController = FindObjectOfType<VideoController>();
        _avProLiveCamera = FindObjectOfType<AVProLiveCamera>();
    }

    private void Start()
    {
        ShowLiveFeed();
        _avProLiveCamera._deviceSelection = AVProLiveCamera.SelectDeviceBy.Name;
        _avProLiveCamera._desiredModeIndex = PlayerPrefs.GetInt("CameraModeIndex");
        _avProLiveCamera._desiredDeviceNames.Add(PlayerPrefs.GetString("CameraName"));
        _avProLiveCamera._desiredFrameRate = Single.MaxValue;
        _avProLiveCamera.Begin();
    }

    public void SetCameraName(string cameraName)
    {
        _avProLiveCamera._desiredDeviceNames.Add(cameraName);
    }

    public void ShowCameraConfigWindow()
    {
        _avProLiveCamera.Device.ShowConfigWindow();
    }
    
    public void EnableDeviceMenu(bool show)
    {
        GetComponent<QuickDeviceMenu>().enabled = show;
    }
    
    public void ShowRecordedVideoForUser()
    {
        VideoFeed.instance.ShowLiveFeed(false);
    }

    public void ShowRecordedVideoOnGUI(bool show)
    {
        if (show)
        {
            _videoController.GetComponentInParent<CanvasGroup>().alpha = 1;
            _videoController.PrepareForUrl("file://" + PathConfig.lastVideoFile);    
        }
        else
        {
            _videoController.GetComponentInParent<CanvasGroup>().alpha = 0;
        }
    }
    
    public void ShowLiveFeed()
    {
        VideoFeed.instance.ShowLiveFeed(true);
    }
    
    
}
