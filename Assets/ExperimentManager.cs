﻿using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;
using VideoPlayer = UnityEngine.Video.VideoPlayer;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    [SerializeField] private PlayableDirector _interventionTimeline;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Text _mirrorsReminderText;
    [SerializeField] private Text _currentPhaseText;
    [SerializeField] private Text _currentTimeText;
    [SerializeField] private GameObject _tcpConnectionCanvas;
    [SerializeField] private Button _playButton;

    public ExperimentData experimentData;
    
    private void Awake()
    {
      if (instance == null) instance = this;
      _startButton.onClick.AddListener(delegate { StartInstructedPhase(); });
      _nextButton.onClick.AddListener(delegate
      {
          SparkSwapInstructionsGUI.instance.Next();
          _mirrorsReminderText.enabled = false;
      });
      
      _playButton.onClick.AddListener(delegate
      {
          if (_interventionTimeline.state == PlayState.Playing)
          {
              _interventionTimeline.Pause();
              _playButton.GetComponentInChildren<Text>().text = "play";
          }
          else if (_interventionTimeline.state == PlayState.Paused)
          {
              _interventionTimeline.Play();    
              _playButton.GetComponentInChildren<Text>().text = "pause";
          }
      });
    }

    private void Start()
    {
        if (ThreatCanvas.instance == null) Instantiate(_tcpConnectionCanvas);
    }

    private void Update()
    {
        _currentTimeText.text = "Current Time : " + Mathf.RoundToInt((float)_interventionTimeline.time) + " seconds";
    }

    public void ReadyForInstructedPhase()
    {
        _startButton.interactable = true;
        _nextButton.interactable = false;
    }

    public void StartFreePhase()
    {
        _currentPhaseText.text = "Current phase : Free Phase";
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Free_Phase");
        SparkSwapInstructionsGUI.instance.ShowInstructionText("Bewegen Sie sich frei aber versuchen Sie die Bewegungen, die Sie sehen, mit Ihren eigenen Bewegungen zu synchronisieren. \n \n Hierzu können Sie versuchen, die Bewegungen entweder selber zu bestimmen oder ihnen zu folgen. \n \n Bitte fangen Sie an und bewegen Sie sich langsam.", 18);
      
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }
    }
    
    public void StartTactilePhase()
    {
        _currentPhaseText.text = "Current phase : Tactile Phase";
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Tactile_Phase");

        //play tactile phase instruction audio or text
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }

        SparkSwapInstructionsGUI.instance.ShowInstructionText("Bitte legen Sie Ihre Hände auf die Knie, und bewegen Sie sich nicht. Ein Versuchsleiter wird Ihnen jetzt langsam über den Arm streichen.", 6);
        Debug.Log("start tactile phase for " + experimentData.conditionType + " " + experimentData.participantType);
    }
    
    public void EndIntervention()
    {
        Debug.Log("End of intervention");
        experimentData.LoadNextScene();
    }
    
    public void StartInstructedPhase()
    {
        _currentPhaseText.text = "Current phase : Instructed Phase";
        SparkSwapInstructionsGUI.instance.ShowInstructionText("Sie können jetzt anfangen.", 3);
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Instructed_Phase");
        VideoFeed.instance.SetDimmed(false);
        _startButton.gameObject.SetActive(false);
        _interventionTimeline.Play();
        
        if (experimentData.conditionType == ConditionType.control)
        {
            //switch to pre recorded video
            string filePath;
            if (experimentData.controlVideos.TryGetValue(experimentData.subjectID, out filePath))
                _videoPlayer.url = filePath;
            else 
                Debug.Log("file not found!");
            if (experimentData.participantType == ParticipantType.follower) VideoCameraManager.instance.ShowRecordedVideoForUser();
            _videoPlayer.Play(); 
        }
        
        Debug.Log("start instructed phase for " + experimentData.conditionType + " " + experimentData.participantType);
    }

}
