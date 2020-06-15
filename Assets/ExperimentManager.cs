using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using VideoPlayer = UnityEngine.Video.VideoPlayer;

public class ExperimentManager : MonoBehaviour
{
    public static ExperimentManager instance;

    [SerializeField] private PlayableDirector _interventionTimeline;
    [SerializeField] private VideoPlayer _videoPlayer;

    [SerializeField] private TrackAsset _leaderTrack;
    [SerializeField] private TrackAsset _followerTrack;

    [SerializeField] private ExperimentData _experimentData;
    
    private void Awake()
    {
      if (instance == null) instance = this;
      OscManager.OnOtherStatus += StartExperiment;
    }

    private void Start()
    {
        TimelineAsset timelineAsset = (TimelineAsset) _interventionTimeline.playableAsset;
        _followerTrack = timelineAsset.GetOutputTrack(3);
        _leaderTrack = timelineAsset.GetOutputTrack(2);
        _experimentData.experimentState = ExperimentState.intervention;
    }

    public void StartExperiment()
    {
        //activate/deactivate clip tracks depending on if leader or follower
        _followerTrack.muted = _experimentData.participantType != ParticipantType.follower;
        _leaderTrack.muted = _experimentData.participantType != ParticipantType.leader; 
        
        _interventionTimeline.Play();
    
        if (_experimentData.conditionType == ConditionType.experimental)
        {
            OscManager.instance.SetSendHeadtracking(true); //enable sending/receiving headtracking
            VideoFeed.instance.twoWayWap = true; //move POV according to other headtracking
        }
        else 
        {
            VideoFeed.instance.twoWayWap = false; //POV follows own headtracking
        }
    
        if (_experimentData.participantType == ParticipantType.follower && _experimentData.conditionType == ConditionType.control)
        {
            string filePath;
            if (_experimentData.controlVideos.TryGetValue(_experimentData.subjectID, out filePath))
                _videoPlayer.url = filePath;
            else 
                Debug.Log("file not found!");
        }
        
    }

    public void StartFreePhase()
    {
        //play free phase instruction audio or text
        if (_experimentData.participantType == ParticipantType.follower && _experimentData.conditionType == ConditionType.control)
        {
            //switch to pre recorded video
            VideoFeed.instance.ShowLiveFeed(false);
            _videoPlayer.Play();
        }
        
        Debug.Log("start free phase for " + _experimentData.conditionType + " " + _experimentData.participantType);
    }

    public void StartTactilePhase()
    {
        //play tactile phase instruction audio or text

        if (_experimentData.participantType == ParticipantType.follower && _experimentData.conditionType == ConditionType.control)
        {
            //switch back to live video
            VideoFeed.instance.ShowLiveFeed(true);
        }
        
        Debug.Log("start tactile phase for " + _experimentData.conditionType + " " + _experimentData.participantType);
    }
    
    public void EndIntervention()
    {
        OscManager.instance.SetSendHeadtracking(false);
        Debug.Log("End of intervention");
        _experimentData.experimentState = ExperimentState.post;
        SceneManager.LoadScene("MotorTest");
    }

}
