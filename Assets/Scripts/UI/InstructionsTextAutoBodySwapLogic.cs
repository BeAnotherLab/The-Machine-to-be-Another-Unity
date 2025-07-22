using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class InstructionsTextAutoBodySwapLogic : MonoBehaviour
{
    public void ExperienceFinished() 
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
    }
}
