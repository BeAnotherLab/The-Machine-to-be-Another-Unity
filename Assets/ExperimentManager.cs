using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental };
public enum ExperimentStep { pre, intervention, post };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    public ParticipantType participantType;
    public ConditionType conditionType;
    public ExperimentStep experimentStep;

    private void Awake()
    {
      if (instance == null) instance = this;
      CognitiveTestManager.instance.OnPreTestsFinished += OnTestFinished;
    }
    
    private void OnTestFinished()
    {
       if (experimentStep == ExperimentStep.pre)
       {
           InterventionManager.instance.ReadyToStart();
       }
    }

    private void OnInterventionFinished()
    {
        experimentStep = ExperimentStep.post;
        CognitiveTestManager.instance.StartTest(experimentStep);
    }
   
}
