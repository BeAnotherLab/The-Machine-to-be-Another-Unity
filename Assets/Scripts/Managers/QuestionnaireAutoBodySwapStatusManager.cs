using ScriptableObjectArchitecture;
using UnityEngine;
using VRStandardAssets.Utils;
using Debug = DebugFile;


public class QuestionnaireAutoBodySwapStatusManager : StatusManager 
{
    #region Private Fields

    [SerializeField] private QuestionnaireStateVariable _questionnaireState; //TODO remove

    private bool _showQuestionnaireThisRound; //TODO remove
    
    #endregion

    #region MonoBehaviour Methods

    #endregion


    #region Public Methods
    
    public new void OtherLeft()
    {
        //if experience started
        if (previousOtherState.Value == UserState.readyToStart)
        {
            //only reset on other left if experience running, post finished, or doing pre questionnaire
            if (_experienceRunning || _questionnaireState.Value != QuestionnaireState.post) //TODO remove questionnaire
            {
                instructionsTimeline.Stop();
                _experienceRunning = false;    
                StartCoroutine(WaitBeforeResetting()); //after a few seconds, reset experience.
                selfState.Value = UserState.headsetOn;    
            }
        }
        Debug.Log("the other user removed the headset", DLogType.Input);
    }

    public void Standby(bool start = false, bool dimOutOnExperienceStart = true)
    {
       base.Standby(start, dimOutOnExperienceStart);
       _showQuestionnaireThisRound = false; //TODO remove
    }
    
    public void EnablePresenceDetection(bool enablePresenceDetection) //TODO remove
    {
        
    }

    public void SelfRemovedHeadset()
    {
        //TODO use event instead 
        _confirmationMenu.GetComponent<VRInteractiveItem>().Out(); //notify the VR interactive element that we are not hovering any more
        if (previousSelfState.Value == UserState.readyToStart 
            && _questionnaireState.Value != QuestionnaireState.post) { //reset unless doing post questionnaire 
            Standby(false, _dimOutOnExperienceStart); //if we were ready and we took off the headset go to initial state
        }
        
        OscManager.instance.SendThisUserStatus(selfState);
        Debug.Log("this user removed his headset", DLogType.Input);
    }

    public void OnBothConsentsGiven(bool bothGiven) 
    {
        _showQuestionnaireThisRound = bothGiven;
        if(!bothGiven) StartPlaying();
    }

    public void OnBothQuestionnaireFinished(QuestionnaireState state) //TODO move to own script
    {
        if (state == QuestionnaireState.pre) StartPlaying();
        else if (state == QuestionnaireState.post) StartCoroutine(WaitBeforeResetting()); 
    }
    
    #endregion


    #region Private Methods

    private void IsOver() //called at the the end of the experience
    {
        base.IsOver();
        _experienceFinishedGameEvent.Raise(_showQuestionnaireThisRound); 
    }


    #endregion
}
 