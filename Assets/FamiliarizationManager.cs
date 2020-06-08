using System;
using System.Collections;
using System.Collections.Generic;
using RockVR.Video;
using RockVR.Video.Demo;
using UnityEngine;
using UnityEngine.Playables;

public class FamiliarizationManager : MonoBehaviour
{
    public static FamiliarizationManager instance;
    
    [SerializeField] private PlayableDirector _familiarizationTimeline;

    [SerializeField] private ExperimentData _experimentData;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void StartFamiliarization()
    {
        CustomVideoCaptureCtrl.instance.StartCapture();
        _familiarizationTimeline.Play();
    }
    
    public void TimelineStopFamiliarization()
    {
        CustomVideoCaptureUI.instance.Next(true);
    }

    public void ButtonStopFamiliarization()
    {
        _familiarizationTimeline.Stop();
    }

    public void StartExperiment()
    {
        
    }

}
