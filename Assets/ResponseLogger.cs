using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class ResponseLogger : MonoBehaviour
{
    [SerializeField] private ResponseData _responseData;
    [SerializeField] private AnswerType _answerType;
    [SerializeField] private QuestionnaireState _questionnaireState;
    [SerializeField] private GameEventBase _newDataAvailableEvent;

    public void NextButtonPressed()
    {
        _responseData.answerType = _answerType;
        _responseData.questionnaireState = _questionnaireState;
        _responseData.timestamp = DateTime.Now;
        _newDataAvailableEvent.Raise();
    }
    
    public void SetValue(bool value)
    {
        if (value) _responseData.answerValue = value.ToString();
        else _responseData.answerValue = value.ToString();
    }

    public void SetValue(float value)
    {
        _responseData.answerValue = value.ToString();
    }

    public void SetValue(string value)
    {
        _responseData.answerValue = value;
    } 
}
