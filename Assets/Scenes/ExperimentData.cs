using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ParticipantType { leader, follower }
public enum ConditionType { control, experimental, familiarization }
public enum ExperimentState { familiarization, threatPre, swap1, threatPost, task1, swap2, task2, task3 }

[CreateAssetMenu]
public class ExperimentData : ScriptableObject
{
    public string subjectID;
    public ConditionType conditionType;
    public ParticipantType participantType;
    public string subjectDirection;
    public ExperimentState experimentState;
    public bool debug;
    public StringStringDictionary controlVideos;
    public string[] tasks;
    public string threatOrder;
    //TODO set in GUI
    public bool mainComputer; //defines if this computer send sync signals to the other in threat task
    
    [SerializeField] private int taskIndex;

    public void LoadNextScene()
    {
        if (experimentState == ExperimentState.familiarization) //after initial scene, load threat pre
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
        else if (taskIndex == 0 && experimentState == ExperimentState.threatPost) //after threat post, load first task
        {
            SceneManager.LoadScene(tasks[taskIndex]);
            experimentState = ExperimentState.task1; 
            taskIndex++;
        }
        else if (taskIndex == 1 &&  experimentState == ExperimentState.task1) //after first task, load second swap
        {
            experimentState = ExperimentState.swap2; 
            SceneManager.LoadScene("SparkSwap");
        }
        else if (experimentState == ExperimentState.swap2) //after second swap, load second task
        {
            experimentState = ExperimentState.task2;
            SceneManager.LoadScene(tasks[taskIndex]);
            taskIndex++;
        }
        else if (experimentState == ExperimentState.task2) //after second task, load third task
        {
            experimentState = ExperimentState.task3;
            SceneManager.LoadScene(tasks[taskIndex]);
            taskIndex++;
        }
        else if (experimentState == ExperimentState.task3) //experiment is over
        {
            SceneManager.LoadScene("End");
        }
    }
    
    public void Clear()
    {
        subjectID = "";
        taskIndex = 0;
        experimentState = ExperimentState.familiarization;
    }
}
