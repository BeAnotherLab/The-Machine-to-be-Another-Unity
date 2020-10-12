using UnityEngine;
using UnityEngine.SceneManagement;

public enum ParticipantType { leader, follower }
public enum ConditionType { control, experimental }
public enum ExperimentState { familiarization, baselinePre, threatPre, swap1, threatPost, task1, swap2, task2, questionnaire, baselinePost }
public enum SubjectDirection {left, right}

[CreateAssetMenu]
public class ExperimentData : ScriptableObject
{
    public string subjectID;
    public ConditionType conditionType;
    public ParticipantType participantType;
    public SubjectDirection subjectDirection;
    public ExperimentState experimentState;
    public bool debug;
    public StringStringDictionary controlVideos;
    public string[] tasks;
    public string threatOrder;
    //TODO set in GUI
    public bool mainComputer; //defines if this computer send sync signals to the other in threat task
    
    public void LoadNextScene()
    {
        if (experimentState == ExperimentState.familiarization) //after initial scene, load baseline 
        {
            SceneManager.LoadScene("BaselinePre");
            experimentState = ExperimentState.baselinePre;
        }
        else if (experimentState == ExperimentState.baselinePre) //after initial scene, load threat pre
        {
            SceneManager.LoadScene("Threat");
            experimentState = ExperimentState.threatPre;
        }
        else if (experimentState == ExperimentState.threatPre) //after threat pre, load first swap 
        {
            SceneManager.LoadScene("SparkSwap");
            experimentState = ExperimentState.swap1;
        }
        else if (experimentState == ExperimentState.swap1) //after first swap, load threat post
        {
            experimentState = ExperimentState.threatPost; 
            SceneManager.LoadScene("Threat");
        }
        else if (experimentState == ExperimentState.threatPost) //after threat post, load first task
        {
            SceneManager.LoadScene(tasks[0]);
            experimentState = ExperimentState.task1; 
        }
        else if (experimentState == ExperimentState.task1) //after first task, load second swap
        {
            experimentState = ExperimentState.swap2; 
            SceneManager.LoadScene("SparkSwap");
        }
        else if (experimentState == ExperimentState.swap2) //after second swap, load second task
        {
            experimentState = ExperimentState.task2;
            SceneManager.LoadScene(tasks[1]);
        }
        else if (experimentState == ExperimentState.task2) //after second task, load third task
        {
            experimentState = ExperimentState.questionnaire;
            SceneManager.LoadScene("Questionnaire");
        }
        else if (experimentState == ExperimentState.questionnaire) //after last task, load last baseline step
        {
            SceneManager.LoadScene("BaselinePost");
            experimentState = ExperimentState.baselinePost;
        }
        else if (experimentState == ExperimentState.baselinePost) //experiment is over
        {
            SceneManager.LoadScene("End");
        }
    }
    
    public void Clear()
    {
        subjectID = "";
        experimentState = ExperimentState.familiarization;
    }
}
