using System;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace RockVR.Video.Demo
{
    public class CustomVideoCaptureUI : MonoBehaviour
    {
        public static CustomVideoCaptureUI instance;
        
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _processingText;

        private bool _captureFinishedOnce; //flag to only do processing finished related actions once since we don't have processing finished events from the Video Capture asset
        private void Awake()
        {
            if (instance == null) instance = this;
            _nextButton.onClick.AddListener(delegate { if (FamiliarizationManager.instance.GetSubjectID() != "") Next(); });
            CustomVideoPlayer.OnVideoFinished += delegate { VideoFinished(); };
        }

        private void Update()
        {
            if (CustomVideoCaptureCtrl.instance.status == CustomVideoCaptureCtrl.StatusType.STOPPED) //disable button while processing
            {
                EnableRecordButton(false);
            } else if (CustomVideoCaptureCtrl.instance.status == CustomVideoCaptureCtrl.StatusType.FINISH && !_captureFinishedOnce) //enable when we're ready
            {
                EnableRecordButton(true);
                _captureFinishedOnce = true;
                _processingText.text = "processed";
                _nextButton.GetComponentInChildren<Text>().text = "play recording";
            }
        }

        public void EnableRecordButton(bool enable) 
        {
            _nextButton.interactable = enable;
        }
        
        public void Next(bool fromTimeline = false)
        { 
            switch (CustomVideoCaptureCtrl.instance.status)
            {
                case  CustomVideoCaptureCtrl.StatusType.NOT_START: // before recording, first time around
                    CustomVideoCaptureCtrl.instance.StartCapture();
                    _captureFinishedOnce = false;
                    _processingText.text = "recording";
                    _nextButton.GetComponentInChildren<Text>().text = "stop recording";
                    FamiliarizationManager.instance.StartFamiliarization();            
                    break;
                case CustomVideoCaptureCtrl.StatusType.STARTED: // while recording, we pressed stop button or recording time is up
                    CustomVideoCaptureCtrl.instance.StopCapture();
                    if (!fromTimeline) FamiliarizationManager.instance.ButtonStopFamiliarization(); //?
                    _processingText.text = "processing";
                    break;
                case CustomVideoCaptureCtrl.StatusType.FINISH: //we're done processing the recorded video
                    CustomVideoPlayer.instance.SetRootFolder();
                    CustomVideoPlayer.instance.PlayVideo();
                    _nextButton.GetComponentInChildren<Text>().text = "stop";
                    _processingText.text = "playing";
                    break;    
            }
        }
        
        private void VideoFinished()
        {
            _processingText.text = "ready to capture";
            _nextButton.GetComponentInChildren<Text>().text = "Start Recording";
            FamiliarizationManager.instance.TimelineStopFamiliarization();
        }

    }
}