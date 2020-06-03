using System;
using System.Collections;
using System.Collections.Generic;
using RockVR.Video;
using UnityEngine;
using UnityEngine.Playables;

public class FamiliarizationManager : MonoBehaviour
{
    public static FamiliarizationManager instance;
    
    [SerializeField] private PlayableDirector _familiarizationTimeline;

    private void Awake()
    {
        if (instance == null) instance = this;    
    }

    public void StartFamiliarization()
    {
        CustomVideoCaptureCtrl.instance.StartCapture();
        _familiarizationTimeline.Play();
    }

    public void StopFamiliarization()
    {
        CustomVideoCaptureCtrl.instance.StopCapture();
        _familiarizationTimeline.Stop();
    }
}
