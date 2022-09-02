using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class InstructionsTextAutoBodySwapLogic : MonoBehaviour
{
    [SerializeField] private GameObject _textGameObject;
    [SerializeField] private UserStateVariable _previousOtherState;
    
    public void ExperienceFinished(bool showQuestionnaire) //TODO remove param?
    {
        GetComponent<InstructionsTextBehavior>().ShowTextFromKey("finished", 3);
    }
}
