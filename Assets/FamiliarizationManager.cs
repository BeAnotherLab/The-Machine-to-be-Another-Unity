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

    public void MovementMessage()
    {
        InstructionsTextBehavior.instance.ShowinstructionsText("Bitte üben Sie die Bewegungen, die wir Ihnen gerade gezeigt haben. Im Moment halten wir Ihre Vision absichtlich schwarz.");
    }

    public void DefaultMessage()
    {
        InstructionsTextBehavior.instance.ShowinstructionsText("Bitte warten Sie einen kurzen Moment. Es werden Vorbereitungen vorgenommen...");
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
        if (condition == "experimental") _experimentData.conditionType = ConditionType.experimental;
        else if (condition == "control") _experimentData.conditionType = ConditionType.control;

        if (participant == "leader")  _experimentData.participantType = ParticipantType.leader;
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
        if (dropdownValue == "motor") _experimentData.taskOrder = FirstTask.motor;
        else if (dropdownValue == "cognitive") _experimentData.taskOrder = FirstTask.cognitive;
    }

    public void SelectThreatOrder(ThreatOrder order)
    {
        _experimentData.threatOrder = order;
    }

    public string GetSubjectID()
    {
        return _experimentData.subjectID;
    }
    
}
