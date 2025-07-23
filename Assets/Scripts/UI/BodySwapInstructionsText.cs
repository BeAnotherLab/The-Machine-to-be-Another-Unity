using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class BodySwapInstructionsText : MonoBehaviour //TODO inherit Instructions text?
{
 
    private void OnEnable()
    {
        StatusManager.InitializeInstructions += InitializeInstructions;
    }

    private void OnDisable()
    {
        StatusManager.InitializeInstructions -= InitializeInstructions;
    }
    
    public void ExperienceFinished(bool ls) 
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
    }
      
    private void InitializeInstructions()
    {
       // GetComponent<FadeController>().FadeInText();
       // GetComponent<FadeController>().FadeOutImages();
    }
}
