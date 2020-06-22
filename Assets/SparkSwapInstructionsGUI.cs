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
        _instructionsText.text = instructions[0];
    }

    public void Next()
    {
        _slideIndex++;
        if (_slideIndex < instructions.Length) _instructionsText.text = instructions[_slideIndex];
        else ExperimentManager.instance.ReadyForFreePhase();
    }
}
