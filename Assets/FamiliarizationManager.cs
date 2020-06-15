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

    public void StartExperiment(string condition, string participant, string subjectID, string direction)
    {
        //TODO parse enum to assign from string
        if (condition == "Experimental") _experimentData.conditionType = ConditionType.experimental;
        else if (condition == "Control") _experimentData.conditionType = ConditionType.control;
        else if (condition == "Familiarization") _experimentData.conditionType = ConditionType.familiarization;

        if (participant == "Leader")  _experimentData.participantType = ParticipantType.leader;
        else  _experimentData.participantType = ParticipantType.follower;
        
        _experimentData.subjectDirection = direction;
        
        var subjectIDOK = SetSubjectID(subjectID);

        string filePath;
        _experimentData.controlVideos.TryGetValue(subjectID, out filePath);
        
        if (_experimentData.conditionType == ConditionType.control &&
            _experimentData.participantType == ParticipantType.follower &&
            !File.Exists(filePath))
        {
            ExperimentSettingsGUI.instance.NotifyVideoNotFoundError();
        }
        else {
            _experimentData.experimentState = ExperimentState.pre;
            SceneManager.LoadScene("MotorTest"); //TODO counter balance
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

        CustomVideoCaptureUI.instance.EnableRecordButton();
        
        return fileOK;
    }

    public string GetSubjectID()
    {
        return _experimentData.subjectID;
    }
    
}
