using System;
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
    [SerializeField] private GameObject _tcpConnectionCanvas;
    private bool _readyForFreePhase;
    
    public ExperimentData experimentData;
    
    private void Awake()
    {
      if (instance == null) instance = this;
      _startButton.onClick.AddListener(delegate { StartInstructedPhase(); });
    }

    private void Start()
    {
        if (ThreatCanvas.instance == null) Instantiate(_tcpConnectionCanvas);
    }

    private void Update()
    {
        if (!_readyForFreePhase)
        {
            if(Input.GetMouseButtonUp(0)) SparkSwapInstructionsGUI.instance.Next();
        }
    }

    public void ReadyForInstructedPhase()
    {
        _startButton.interactable = true;
    }

    public void StartFreePhase()
    {
        SparkSwapInstructionsGUI.instance.ShowInstructionText("Please start moving slowly, the other person will try to follow your movement. Please move slowly", 6);
      
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }
    }
    
    public void StartTactilePhase()
    {
        //play tactile phase instruction audio or text
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }

        SparkSwapInstructionsGUI.instance.ShowInstructionText("Please stay still. Someone will come and gently stroke your arm.", 6);
        Debug.Log("start tactile phase for " + experimentData.conditionType + " " + experimentData.participantType);
    }
    
    public void EndIntervention()
    {
        OscManager.instance.SetSendHeadtracking(false);
        Debug.Log("End of intervention");
        experimentData.LoadNextScene();
    }
    
    public void StartInstructedPhase()
    {
        VideoFeed.instance.SetDimmed(false);
        _startButton.gameObject.SetActive(false);
        _interventionTimeline.Play();
        SparkSwapInstructionsGUI.instance.ShowInstructionText(false);
        
        if (experimentData.conditionType == ConditionType.experimental)
        {
            OscManager.instance.SetSendHeadtracking(true); //enable sending/receiving headtracking
            VideoFeed.instance.twoWayWap = true; //move POV according to other headtracking
        }
        else if (experimentData.conditionType == ConditionType.control)
        {
            VideoFeed.instance.twoWayWap = false; //POV follows own headtracking
            //switch to pre recorded video
            string filePath;
            if (experimentData.controlVideos.TryGetValue(experimentData.subjectID, out filePath))
                _videoPlayer.url = filePath;
            else 
                Debug.Log("file not found!");
            VideoCameraManager.instance.ShowRecordedVideo();
            _videoPlayer.Play();
        }
        
        Debug.Log("start instructed phase for " + experimentData.conditionType + " " + experimentData.participantType);
    }

}
