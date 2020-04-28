using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsTextBehavior : MonoBehaviour
{

    public static InstructionsTextBehavior instance;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }
    
    public void ShowInstructionText(bool show, string text = "")
    {
        transform.parent.gameObject.SetActive(show); //Show instructions canvas
        GetComponent<Text>().text = text; //give feedback
    }

    public void ShowInstructionText(string text, int time)
    {
        StartCoroutine(TimedTextCoroutine(text, time));
    }

    private IEnumerator TimedTextCoroutine(string text, int time)
    {
        ShowInstructionText(true, text);
        yield return new WaitForSeconds(time);
        ShowInstructionText(false);       
    }
    
 
    
}
