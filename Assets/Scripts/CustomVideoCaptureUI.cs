using System;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace RockVR.Video.Demo
{
    public class CustomVideoCaptureUI : MonoBehaviour
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _processingText;

        private bool _captureFinishedOnce; //flag to only do processing finished related actions once since we don't have processing finished events from the Video Capture asset
        private bool _videoPlayed;
        private void Awake()
        {
            _nextButton.onClick.AddListener(delegate
            {
                if (ExperimentSettingsGUI.instance.subjectIDValidated) Next();
            });
            CustomVideoPlayer.OnVideoFinished += delegate { VideoFinished(); };
        }

        private void Update()
        {
            if (CustomVideoCaptureCtrl.instance.status == CustomVideoCaptureCtrl.StatusType.STOPPED) //disable button while processing
            {
                _nextButton.enabled = false;
            } else if (CustomVideoCaptureCtrl.instance.status == CustomVideoCaptureCtrl.StatusType.FINISH && !_captureFinishedOnce) //enable when we're ready
            {
                _nextButton.enabled = true;
                _captureFinishedOnce = true;
                _processingText.text = "processed";
                _nextButton.GetComponentInChildren<Text>().text = "play recording";
            }
        }

        private void Next()
        {
            switch (CustomVideoCaptureCtrl.instance.status)
            {
                case  CustomVideoCaptureCtrl.StatusType.NOT_START: // before recording, first time around
                    StartRecording();            
                    break;
                case CustomVideoCaptureCtrl.StatusType.STARTED: // while recording
                    _processingText.text = "processing";
                    break;
                case CustomVideoCaptureCtrl.StatusType.FINISH: //we're done processing the recorded video
                    if (!_videoPlayed) //if we haven't played the video yet (we only play it once
                    {
                        CustomVideoPlayer.instance.SetRootFolder();
                        CustomVideoPlayer.instance.PlayVideo();
                        _processingText.text = "playing";
                        _videoPlayed = true;
                    }
                    else StartRecording();
                    break;    
            }
        }

        public void StartRecording()
        {
            _videoPlayed = false;
            CustomVideoCaptureCtrl.instance.StartCapture();
            _captureFinishedOnce = false;
            _processingText.text = "recording";
            _nextButton.GetComponentInChildren<Text>().text = "stop recording";
            FamiliarizationManager.instance.StartFamiliarization();
        }
        
        private void VideoFinished()
        {
            _processingText.text = "ready to capture";
            _nextButton.GetComponentInChildren<Text>().text = "Start Recording";
            FamiliarizationManager.instance.StopFamiliarization();
        }

    }
}