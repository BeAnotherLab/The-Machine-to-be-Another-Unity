using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsTextBehavior : MonoBehaviour
{

    public static InstructionsTextBehavior instance;

    [SerializeField] private GameObject _textGameObject;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    #region  Public methods
    
    public void ShowTextFromKey(string key)
    {
        GetComponent<CanvasGroup>().alpha = 1;
        _textGameObject.GetComponent<LocalizedText>().SetTextFromKey(key);
    }

    public void ShowTextFromKey(string key, int time)
    {
        StartCoroutine(TimedTextKeyCoroutine(key, time));
    }
    
    public void ShowInstructionText(bool show, string text = "")
    {
        GetComponent <CanvasGroup>().alpha = show ? 1 : 0;
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
