using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RockVR.Video;
using RockVR.Video.Demo;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class FamiliarizationManager : MonoBehaviour
{
    public static FamiliarizationManager instance;
    [SerializeField] private PlayableDirector _familiarizationTimeline;
    [SerializeField] private ExperimentData _experimentData;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _experimentData.Clear();
    }

    public void SetVideoCapturePath(string filePath)
    {
        _experimentData.controlVideos.Remove(GetSubjectID());
        _experimentData.controlVideos.Add(GetSubjectID(), filePath);
    }
    
    public void StartFamiliarization()
    {
        CustomVideoCaptureCtrl.instance.StartCapture();
        _familiarizationTimeline.Play();
    }
    
    public void TimelineStopFamiliarization()
    {
        CustomVideoCaptureUI.instance.Next(true);
    }

    public void ButtonStopFamiliarization()
    {
        _familiarizationTimeline.Stop();
    }

    public void StartExperiment(string condition, string participant, string subjectID)
    {
        //TODO parse enum to assign from string
        if (condition == "Experimental") _experimentData.conditionType = ConditionType.experimental;
        else if (condition == "Control") _experimentData.conditionType = ConditionType.control;

        if (participant == "Leader")  _experimentData.participantType = ParticipantType.leader;
        else  _experimentData.participantType = ParticipantType.follower;
        
        string filePath;
        _experimentData.controlVideos.TryGetValue(subjectID, out filePath);
        
        if (_experimentData.conditionType == ConditionType.control &&
            _experimentData.participantType == ParticipantType.follower &&
            !File.Exists(filePath)) 
        {
            ExperimentSettingsGUI.instance.NotifyVideoNotFoundError(); //if in control follower condition, notify video not found before starting
        }
        else {
            _experimentData.LoadNextScene();
        }
    }
    
    public bool SetSubjectID(string id)  
    {
        _experimentData.subjectID = id;
    
        //TODO implemeent properly later if needed
        string cognitiveFilePath = Application.dataPath + "/" + "CognitiveTest" + id + "_log.json";
        string motorFilePath = Application.dataPath + "/" + "MotorTest" + id + "_log.json";

        var fileOK = File.Exists(cognitiveFilePath) || File.Exists(motorFilePath);
        if (fileOK) ExperimentSettingsGUI.instance.ShowExistingSubjectIDError();

        CustomVideoCaptureUI.instance.EnableRecordButton(true);
        
        return fileOK;
    }
    
    public void SelectTaskOrder(string dropdownValue)
    {
        if (dropdownValue == "motor test") _experimentData.taskOrder = FirstTask.motor;
        else if (dropdownValue == "cognitive test") _experimentData.taskOrder = FirstTask.cognitive;
    }

    public void SelectThreatOrder(string order)
    {
        _experimentData.threatOrder = order;
    }

    public string GetSubjectID()
    {
        return _experimentData.subjectID;
    }
    
}
