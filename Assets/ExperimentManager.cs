using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    public ParticipantType participant;
    public ConditionType condition;

    [SerializeField] private PlayableDirector _interventionTimeline;
    [SerializeField] private PlayableDirector _familiarizationTimeline;
    
    private void Awake()
    {
      if (instance == null) instance = this;
    }
    
    public void StartExperiment(bool familiarization = false)
    {
        _interventionTimeline.Play();
        
        if (condition == ConditionType.experimental)
        {
            OscManager.instance.sendHeadTracking = true; //enable sending/receiving headtracking
            VideoFeed.instance.twoWayWap = true; //move POV according to other headtracking
        }
        else
        {
            VideoFeed.instance.twoWayWap = false; //POV follows own headtracking
        }
        
        if (participant == ParticipantType.follower)
        {
            //play follower free phase instruction audio or text
        }
        else
        {
            //play leader free phase instruction audio or text
        }
    }

    public void StartFreePhase()
    {
        //play free phase instruction audio or text
        if (participant == ParticipantType.follower && condition == ConditionType.control)
        {
            //switch to pre recorded video
        }
        
        Debug.Log("start free phase for " + condition + " " + participant);
    }

    public void StartTactilePhase()
    {
        //play tactile phase instruction audio or text

        if (participant == ParticipantType.follower && condition == ConditionType.control)
        {
            //switch back to live video
        }
        
        Debug.Log("start tactile phase for " + condition + " " + participant);
    }
    
    public void EndIntervention()
    {
        OscManager.instance.sendHeadTracking = false;
    }

}
