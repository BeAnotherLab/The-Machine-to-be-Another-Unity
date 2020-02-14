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
        
        private bool isPlayVideo;
        private void Awake()
        {
            _nextButton.onClick.AddListener(() => Next());
            Application.runInBackground = true;
            isPlayVideo = false;
        }

        private void Update()
        {
            if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.STOPPED) //disable button while processing
            {
                _nextButton.enabled = false;
            } else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.FINISH) //enable when we're ready
            {
                _nextButton.enabled = true;
                _processingText.text = "processing finished";
                _nextButton.GetComponentInChildren<Text>().text = "play recording";
            }
        }

        private void Next()
        {
            switch (VideoCaptureCtrl.instance.status)
            {
                case  VideoCaptureCtrl.StatusType.NOT_START: // before recording
                    VideoCaptureCtrl.instance.StartCapture();
                    _processingText.text = "recording";
                    _nextButton.GetComponentInChildren<Text>().text = "stop recording";
                    break;
                case VideoCaptureCtrl.StatusType.STARTED: // while recording
                    VideoCaptureCtrl.instance.StopCapture();
                    _processingText.text = "processing";
                    break;
                case VideoCaptureCtrl.StatusType.FINISH: //recording is finished processing
                    if (isPlayVideo) //either video is playing
                    {
                        _processingText.text = "ready to capture";
                        _nextButton.GetComponentInChildren<Text>().text = "stop video";
                        VideoPlayer.instance.StopVideo();
                        isPlayVideo = false;
                    }
                    else //or we haven't played it yet
                    {
                        isPlayVideo = true;
                        VideoPlayer.instance.SetRootFolder();
                        VideoPlayer.instance.PlayVideo();
                        _processingText.text = "playing";                  
                    }
                    break;   
            }
        }

    }
}