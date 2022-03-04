using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum QuestionnaireState
{
    init, //on app init
    pre, //video consent answer was given
    post //questionnaire pre is done
};

public enum AnswerType {
    videoConsent,
    age,
    gender,
    closeness,
    intimacy,
    selfOther,
    embodiment,
    boundaries,
    understanding
}

[CreateAssetMenu]
public class ResponseData : ScriptableObject
{   
    public string subjectID;   
    public string pairID;
    public AnswerType answerType;
    public string answerValue;
    public QuestionnaireState questionnaireState;
    public DateTime timestamp;
}
