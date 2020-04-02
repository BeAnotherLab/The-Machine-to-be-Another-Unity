using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterventionManager : MonoBehaviour
{
    public static InterventionManager instance;

    public enum ParticipantType { leader, follower };
    public ParticipantType participantType;
    
    public enum ExperimentType { control, experimental };
    public ExperimentType experimentType;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void StartInterventon()
    {
        //enable sending/receiving headtracking
        OscManager.instance.sendHeadTracking = true;
        
        //start timing experience
    }

    public void EndIntervention()
    {
        OscManager.instance.sendHeadTracking = false;
        
        //run tests second time, skip practice 
        CognitiveTestManager.instance.StartTest(CognitiveTestManager.steps.testing);
    }

}
