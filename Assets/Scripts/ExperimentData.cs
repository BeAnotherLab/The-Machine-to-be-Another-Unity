using UnityEngine;

public enum ParticipantType { leader, follower, free }
public enum ExperimentState { curtainDown, curtainUp, noVR }

public enum QuestionnaireAnswerType {
    closeness,
    intimacy,
    selfOther,
    embodiment,
    boundaries,
    understanding
}

[CreateAssetMenu]
public class ExperimentData : ScriptableObject
{
    [Header("Experiment Data")]
    public string subjectID;
    public string otherID;
    
    public QuestionnaireAnswerType answerType;
    public string answerValue;

    public ParticipantType participantType;
}
