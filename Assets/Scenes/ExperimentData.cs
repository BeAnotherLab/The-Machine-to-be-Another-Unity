using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental, familiarization };
public enum ExperimentState { familiarization, swap1, taskBlock1, swap2, taskBlock2};

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

    [SerializeField] private int taskIndex;

    public void LoadNextScene()
    {
        if (experimentState == ExperimentState.familiarization) //after initial scene, load first swap
        {
            SceneManager.LoadScene("SparkSwap");
            experimentState = ExperimentState.swap1;
        }
        else if (experimentState == ExperimentState.swap1) //after first swap, load first task
        {
            experimentState = ExperimentState.taskBlock1; 
            SceneManager.LoadScene(tasks[0]);
        }
        else if (taskIndex == 0 && experimentState == ExperimentState.taskBlock1) //after first task, load second task
        {
            taskIndex++;
            SceneManager.LoadScene(tasks[taskIndex]);
        }
        else if (taskIndex == 1 &&  experimentState == ExperimentState.taskBlock1) //after second task, load second swap
        {
            experimentState = ExperimentState.swap2; 
            SceneManager.LoadScene("SparkSwap");
        }
        else if (experimentState == ExperimentState.swap2) //after second swap, load first task of second block
        {
            experimentState = ExperimentState.taskBlock2;
            taskIndex++;
            SceneManager.LoadScene(tasks[taskIndex]);
        }
        else if (taskIndex == 2) //after third task, load last task
        {
            taskIndex++;
            SceneManager.LoadScene(tasks[taskIndex]);
        }
        else if (taskIndex == 3) //experiment is over
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
