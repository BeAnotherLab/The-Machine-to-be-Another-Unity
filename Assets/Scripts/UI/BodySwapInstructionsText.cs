using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapInstructionsText : MonoBehaviour //TODO inherit Instructions text?
{
    [SerializeField] private GameObject _instructionsImages;

    private void Start()
    {
        _instructionsImages.GetComponent<PanelDimmer>().Hide();
    }

    public void FadeInImages() //called by timeline
    {
        _instructionsImages.GetComponent<PanelDimmer>().Show();
    }
    
    public void ExperienceFinished(bool ls) 
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
        _instructionsImages.GetComponent<PanelDimmer>().Hide(); //TODO not here!
    }

    public void Standby()
    {
        _instructionsImages.GetComponent<PanelDimmer>().Hide();
    }
    
    public void OtherUserStateChanged(UserState otherUserState)
    {
        if (otherUserState == UserState.headsetOff)
        {
            _instructionsImages.GetComponent<PanelDimmer>().Hide();
        }
    }
}
