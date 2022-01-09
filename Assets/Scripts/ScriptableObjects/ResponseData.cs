using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum QuestionnaireState
{
    videoConsent,
    pre,
    post
};

public enum AnswerType {
    videoConsent,
    age,
    gender,
    closeness,
    intimacy,
    selfOther,
    embodiment1,
    embodiment2,
    embodiment3,
    embodiment4,
    embodiment5,
    embodiment6,
    boundaries}

[CreateAssetMenu]
public class ResponseData : ScriptableObject
{
    public string userID;   
    public string pairID;
    public AnswerType answerType;
    public string answerValue;
    public QuestionnaireState questionnaireState;

}
