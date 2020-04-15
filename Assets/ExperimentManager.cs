using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    public ParticipantType participantType;
    public ConditionType conditionType;

    private void Awake()
    {
      if (instance == null) instance = this;
    }

}
