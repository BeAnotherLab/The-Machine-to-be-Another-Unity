using System.Collections;
using System.Collections.Generic;
using Lean.Localization;
using ScriptableObjectArchitecture;
using UnityEngine;

public class InstructionsTextAutoBodySwapLogic : MonoBehaviour
{
    [SerializeField] private GameObject _textGameObject;
    [SerializeField] private UserStateVariable _previousOtherState;

    
    public void ExperienceFinished(bool showQuestionnaire)
    {
        _textGameObject.GetComponent<LeanLocalizedText>().TranslationName = "finished";
    }
    
    public void OtherStateChanged(UserState newState) 
    {
        if (_previousOtherState == UserState.readyToStart 
            && newState == UserState.headsetOff)
        {
            GetComponent<InstructionsTextBehavior>().ShowTextFromKey("otherIsGone", 4);
        }
    }
}
