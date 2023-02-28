using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    public QuestionnaireAnswerType _answerType;
    public string answerValue;

    public ParticipantType participantType;
    
    public ExperimentState experimentState;

}
