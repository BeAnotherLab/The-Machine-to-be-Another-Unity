using Lean.Localization;
using UnityEngine;
using ScriptableObjectArchitecture;

public class InstructionsTextQuestionnaireAutoSwapLogic : MonoBehaviour //this is the logic for autobodyswap with questionnaire
{
    [SerializeField] private GameObject _textGameObject;
    [SerializeField] private UserStateVariable _previousOtherState;
    [SerializeField] private QuestionnaireStateVariable _questionnaireState;

    
    public void ExperienceFinished(bool showQuestionnaire)
    {
        if (!showQuestionnaire)
            _textGameObject.GetComponent<LeanLocalizedText>().TranslationName = "finished";
    }
    
    public void OtherStateChanged(UserState newState) 
    {
        if (_previousOtherState == UserState.readyToStart 
            && newState == UserState.headsetOff
            && _questionnaireState.Value != QuestionnaireState.post) //if user removed headset
        {
           GetComponent<InstructionsTextBehavior>().ShowTextFromKey("otherIsGone", 4);
        }
    }

}
