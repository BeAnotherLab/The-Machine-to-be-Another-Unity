using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExperimentStateUI : MonoBehaviour
{
    [SerializeField] private Text _phaseText;
    [SerializeField] private Text _phaseTimeText;
    [SerializeField] private Text _roleText;
    
    public void ElapsedTimeUpdated(float elapsedTime)
    {
        _phaseTimeText.text = elapsedTime + "s";
    }
    
    public void SetRole(int role)
    {
        if (role == 0) _roleText.text = ParticipantType.leader.ToString();
        else if (role == 1) _roleText.text = ParticipantType.follower.ToString();
        else if (role == 2) _roleText.text = ParticipantType.free.ToString();
    }
    
    public void SetPhase(int state)
    {
        if (state == 0) _phaseText.text = ExperimentState.curtainDown.ToString();
        else if (state == 1) _phaseText.text = ExperimentState.curtainUp.ToString();
        else if (state == 2) _phaseText.text = ExperimentState.noVR.ToString();
    }

    public void NoVR(bool noVR)
    {
        if (noVR) _phaseText.text = ExperimentState.noVR.ToString();
    }
    
    public void CurtainOn(bool on)
    {
        if (on) _phaseText.text = ExperimentState.curtainDown.ToString();
        else _phaseText.text = ExperimentState.curtainUp.ToString();
    }
}
