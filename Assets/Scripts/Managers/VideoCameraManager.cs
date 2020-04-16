using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using RockVR.Video;
using UnityEngine;
using UnityEngine.UI;

public class VideoCameraManager : AbstractAVProLiveCameraSwitcher
{
    public static VideoCameraManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        ShowLiveFeed();
    }

    // Update is called once per frame
    private void Update()
    {
       
    }

    public void ShowRecordedVideo()
    {
        VideoFeed.instance.ShowLiveFeed(false);
    }
    
    public void ShowLiveFeed()
    {
        VideoFeed.instance.ShowLiveFeed(true);
    }
}
