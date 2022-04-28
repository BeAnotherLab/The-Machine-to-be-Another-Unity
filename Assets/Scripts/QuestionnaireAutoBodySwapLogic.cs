using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class QuestionnaireAutoBodySwapLogic : MonoBehaviour
{
    [SerializeField] private UserStateVariable _previousSelfState;
    [SerializeField] private UserStateVariable _previousOtherState;
    
    [SerializeField] private QuestionnaireStateVariable _questionnaireState;
    
    public void SelfStateChanged(UserState newState) //if self removed headset during post, do as if it had finished
    {
        if (_previousSelfState == UserState.readyToStart 
            && newState == UserState.headsetOff
            && _questionnaireState == QuestionnaireState.post) //if user removed headset
        {
            GetComponent<QuestionnaireUI>().EndPostQuestionnaire();
        }
    }
    
    public void OtherStateChanged(UserState newState) //Hide questionnaire if we're doing questionnaire pre and the other removed the headset.
    {
        if (_previousOtherState == UserState.readyToStart 
            && newState == UserState.headsetOff
            && _questionnaireState == QuestionnaireState.pre) //if user removed headset
        {
            GetComponent<QuestionnaireUI>().EndPreQuestionnaire();
        }
    }

}
