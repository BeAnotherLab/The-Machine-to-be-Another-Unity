using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class InterventionManager : MonoBehaviour
{
    public static InterventionManager instance;

    private PlayableDirector _interventionTimeline;
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _interventionTimeline = GetComponentInChildren<PlayableDirector>();
    }

    private void Start()
    {
        CognitiveTestManager.instance.OnPreTestsFinished += ReadyToStart;
    }

    public void ReadyToStart()
    {
        //enable sending/receiving headtracking
        OscManager.instance.sendHeadTracking = true;
        
        _interventionTimeline.Play();
        //start timing experience
    }

    public void EndIntervention()
    {
        OscManager.instance.sendHeadTracking = false;
        
        //say we're doing cognitive test again
        
        //start cognitive test again
        CognitiveTestManager.instance.StartTest(ExperimentStep.post);        
    }
    
}
