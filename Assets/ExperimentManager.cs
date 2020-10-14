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
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Free_Phase");
        SparkSwapInstructionsGUI.instance.ShowInstructionText("Bewegen Sie sich frei aber versuchen Sie die Bewegungen, die Sie sehen, mit Ihren eigenen Bewegungen zu synchronisieren. \n \n Hierzu können Sie versuchen die Bewegungen entweder selber führen oder folgen.\n \n Bitte fangen Sie an und bewegen Sie sich langsam an.", 18);
      
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }
    }
    
    public void StartTactilePhase()
    {
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Tactile_Phase");

        //play tactile phase instruction audio or text
        if (experimentData.participantType == ParticipantType.follower && experimentData.conditionType == ConditionType.control)
        {
            VideoCameraManager.instance.ShowLiveFeed(); //switch back to live video
        }

        SparkSwapInstructionsGUI.instance.ShowInstructionText("Bitte bleiben Sie still. Ein Versuchsleiter wird Ihnen jetzt langsam über den Arm streichen.", 6);
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
        TCPClient.instance.SendTCPMessage(experimentData.experimentState + "_Instructed_Phase");
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
            if (experimentData.participantType == ParticipantType.follower) VideoCameraManager.instance.ShowRecordedVideoForUser();
            _videoPlayer.Play(); 
        }
        
        Debug.Log("start instructed phase for " + experimentData.conditionType + " " + experimentData.participantType);
    }

}
