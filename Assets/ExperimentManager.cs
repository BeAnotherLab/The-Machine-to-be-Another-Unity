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

    public ParticipantType participantType;
    public ConditionType conditionType;

    [SerializeField] private PlayableDirector _controlTimeline;
    [SerializeField] private PlayableDirector _experimentalTimeline;
    [SerializeField] private PlayableDirector _familiarizationTimeline;
    
    private void Awake()
    {
      if (instance == null) instance = this;
    }

    public void StartExperiment(ParticipantType participant, ConditionType condition)
    {
        
    }
    
}
