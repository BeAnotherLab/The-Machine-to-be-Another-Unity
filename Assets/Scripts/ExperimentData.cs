using System;
using UnityEngine;

public enum ParticipantType { leader, follower, free }
public enum ExperimentState { curtainDown, curtainUp, noVR }

public enum QuestionnaireAnswerType {
    selfOther,
    boundaries,
    sync1,
    sync2,
    sync3, 
    sync4
}

[CreateAssetMenu]
public class ExperimentData : ScriptableObject
{
    [Header("Experiment Data")]
    public string subjectID;
    public string otherID;
    
    public QuestionnaireAnswerType answerType;
    
    public ExperimentState _experimentState;
    public string answerValue;

    public ParticipantType participantType;
    public DateTime timestamp;
}
