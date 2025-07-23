using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class InstructionsTextBodySwapLogic : MonoBehaviour //TODO inherit Instructions text?
{
    public void ExperienceFinished(bool ls) 
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
    }
      
}
