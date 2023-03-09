using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class ResponseLogger : MonoBehaviour
{
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private QuestionnaireAnswerType _answerType;
    [SerializeField] private GameEventBase _newDataAvailableEvent;

    public void NextButtonPressed()
    {
        _experimentData.answerType = _answerType;
        _experimentData.timestamp = DateTime.Now;
        _newDataAvailableEvent.Raise();
    }
    
    public void SetValue(bool value)
    {
        if (value) _experimentData.answerValue = value.ToString();
        else _experimentData.answerValue = value.ToString();
    }

    public void SetValue(float value)
    {
        _experimentData.answerValue = value.ToString();
    }

    public void SetValue(string value)
    {
        _experimentData.answerValue = value;
    } 
}
