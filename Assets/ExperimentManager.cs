using System;
using System.Collections;
using System.Collections.Generic;
using RockVR.Video;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VideoPlayer = UnityEngine.Video.VideoPlayer;

public enum ParticipantType { leader, follower };
public enum ConditionType { control, experimental, familiarization };

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    public ParticipantType participant;
    public ConditionType condition;

    [SerializeField] private PlayableDirector _interventionTimeline;
    [SerializeField] private PlayableDirector _familiarizationTimeline;
    [SerializeField] private VideoPlayer _videoPlayer;

    [SerializeField] private TrackAsset _leaderTrack;
    [SerializeField] private TrackAsset _followerTrack;
    
    private void Awake()
    {
      if (instance == null) instance = this;
      OscManager.OnOtherStatus += StartExperiment;
    }

    private void Start()
    {
        TimelineAsset timelineAsset = (TimelineAsset) _interventionTimeline.playableAsset;
        _followerTrack = timelineAsset.GetOutputTrack(1);
        _leaderTrack = timelineAsset.GetOutputTrack(2);        
    }

    public void StartExperiment()
    {
        ExperimentSettingsGUI.instance.gameObject.SetActive(false); //disable experiment GUI
        
        //activate/deactivate clip tracks depending on if leader or follower
        _followerTrack.muted = participant != ParticipantType.follower;
        _leaderTrack.muted = participant != ParticipantType.leader; 
        
        if (condition != ConditionType.familiarization)
        {
            _interventionTimeline.Play();
        
            if (condition == ConditionType.experimental)
            {
                OscManager.instance.sendHeadTracking = true; //enable sending/receiving headtracking
                VideoFeed.instance.twoWayWap = true; //move POV according to other headtracking
            }
            else 
            {
                VideoFeed.instance.twoWayWap = false; //POV follows own headtracking
            }
        
            if (participant == ParticipantType.follower && condition == ConditionType.control)
            {
                var currentSubjectID = PlayerPrefs.GetString("SubjectID");
                _videoPlayer.url = PlayerPrefs.GetString("VideoCapturePath" + currentSubjectID);
            }
        }
        else
        {
            if(participant == ParticipantType.follower) VideoCaptureCtrl.instance.StartCapture();
            _familiarizationTimeline.Play();
        }
    }

    public void StartFreePhase()
    {
        //play free phase instruction audio or text
        if (participant == ParticipantType.follower && condition == ConditionType.control)
        {
            //switch to pre recorded video
            VideoFeed.instance.ShowLiveFeed(false);
            _videoPlayer.Play();
        }
        
        Debug.Log("start free phase for " + condition + " " + participant);
    }

    public void StartTactilePhase()
    {
        //play tactile phase instruction audio or text

        if (participant == ParticipantType.follower && condition == ConditionType.control)
        {
            //switch back to live video
            VideoFeed.instance.ShowLiveFeed(true);
        }
        
        Debug.Log("start tactile phase for " + condition + " " + participant);
    }
    
    public void EndIntervention()
    {
        OscManager.instance.sendHeadTracking = false;
        
        Debug.Log("End of intervention");
    }

}
