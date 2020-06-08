using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental, familiarization };
public enum ExperimentState { familiarization, pre, intervention, post };

[CreateAssetMenu]
public class ExperimentData : ScriptableObject
{
    public string subjectID;
    public ConditionType conditionType;
    public ParticipantType participantType;
    public string subjectDirection;
    public ExperimentState experimentState;

    public void Clear()
    {
        subjectID = "";
        experimentState = ExperimentState.familiarization;
    }
}
