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
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _videoController = FindObjectOfType<VideoController>();
        CustomVideoCaptureCtrl.instance.eventDelegate.OnComplete += ShowRecordedVideoOnGUI;
    }

    private void Start()
    {
        ShowLiveFeed();
    }

    public void EnableDeviceMenu(bool show)
    {
        GetComponent<QuickDeviceMenu>().enabled = show;
    }
    
    public void ShowRecordedVideoForUser()
    {
        VideoFeed.instance.ShowLiveFeed(false);
    }

    public void ShowRecordedVideoOnGUI()
    {
        _videoController.GetComponentInParent<CanvasGroup>().alpha = 1;
        _videoController.PrepareForUrl("file://" + PathConfig.lastVideoFile);
    }
    
    public void ShowLiveFeed()
    {
        VideoFeed.instance.ShowLiveFeed(true);
    }
}
