﻿using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsTextBehavior : MonoBehaviour
{

    public static InstructionsTextBehavior instance;

    [SerializeField] private GameObject _textGameObject;

    [SerializeField] private UserStateVariable _previousOtherState;
    [SerializeField] private QuestionnaireStateVariable _questionnaireState;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #region  Public methods
    
    public void ExperienceFinished(bool showQuestionnaire)
    {
        if(!showQuestionnaire) ShowTextFromKey("finished");
    }
    
    public void ShowTextFromKey(string key)
    {
        GetComponent<PanelDimmer>().Show();
        _textGameObject.GetComponent<LocalizedText>().SetTextFromKey(key);
    }

    public void ShowTextFromKey(string key, int time)
    {
        StartCoroutine(TimedTextKeyCoroutine(key, time));
    }
    
    public void ShowInstructionText(bool show, string text = "")
    {
        GetComponent<PanelDimmer>().Show(show);
        _textGameObject.GetComponent<Text>().text = text; //give feedback
    }

    public void ShowinstructionsText(string text)
    {
        _textGameObject.GetComponent<Text>().text = text; //give feedback
    }
    
    public void ShowInstructionText(string text, int time)
    {
        StartCoroutine(TimedTextCoroutine(text, time));
    }

    public void OtherStateChanged(UserState newState) 
    {
        if (_previousOtherState == UserState.readyToStart 
            && newState == UserState.headsetOff
            && _questionnaireState.Value != QuestionnaireState.post) //if user removed headset
        {
            ShowInstructionText("It seems like the other user left. Thanks for your participation!", 4);
        }
    }
    
    #endregion
    
    #region Private Methods
    
    private IEnumerator TimedTextCoroutine(string text, int time)
    {
        ShowInstructionText(true, text);
        yield return new WaitForSeconds(time);
        ShowInstructionText(false);       
    }

    private IEnumerator TimedTextKeyCoroutine(string key, int time)
    {
        ShowTextFromKey(key);
        yield return new WaitForSeconds(time);
        ShowInstructionText(false);
    }
    
    #endregion
}
