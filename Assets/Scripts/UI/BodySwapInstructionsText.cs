using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapInstructionsText : MonoBehaviour //TODO inherit Instructions text?
{
    [SerializeField] private GameObject _instructionsImages;

    private void OnEnable()
    {
        StatusManager.InitializeInstructions += InitializeInstructions;
    }

    private void OnDisable()
    {
        StatusManager.InitializeInstructions -= InitializeInstructions;
    }

    public void FadeInImages() //called by timeline
    {
        _instructionsImages.GetComponent<PanelDimmer>().Show();
    }
    
    public void ExperienceFinished(bool ls) 
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
    }
      
    private void InitializeInstructions()
    {
        _instructionsImages.GetComponent<PanelDimmer>().Hide();
       // GetComponent<FadeController>().FadeInText();
    }
}
