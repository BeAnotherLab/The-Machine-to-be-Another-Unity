using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SparkSwapInstructionsGUI : MonoBehaviour
{
    public static SparkSwapInstructionsGUI instance;

    [SerializeField] private Text _instructionsText;
    private int _slideIndex;

    [SerializeField] private string[] leaderInstructions;
    [SerializeField] private string[] followerInstructions;

    private string[] instructions;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (ExperimentManager.instance.experimentData.participantType == ParticipantType.leader) instructions = leaderInstructions;
        if (ExperimentManager.instance.experimentData.participantType == ParticipantType.follower) instructions = followerInstructions;
        ShowInstructionText(false);
    }

    public void Next()
    {
        if (_slideIndex == 0)
        {
            ShowInstructionText(true);
            _instructionsText.text = instructions[0];
            _slideIndex++;
        }
        else
        {
            if (_slideIndex < instructions.Length) _instructionsText.text = instructions[_slideIndex];
            else ExperimentManager.instance.ReadyForInstructedPhase();
            _slideIndex++;
        }
    }
    
    public void ShowInstructionText(bool show, string text = "")
    {
        GetComponent <CanvasGroup>().alpha = show ? 1 : 0;
        _instructionsText.GetComponent<Text>().text = text; //give feedback
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
