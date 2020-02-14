using System;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;

namespace RockVR.Video.Demo
{
    public class CustomVideoCaptureUI : MonoBehaviour
    {
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _processingText;

        private bool isPlayVideo;
        private void Awake()
        {
            _nextButton.onClick.AddListener(() => { Next()});
            Application.runInBackground = true;
            isPlayVideo = false;
            
        }

        private void Start()
        {
            _stopRecordingButton.interactable = false;
            _playVideosButton.interactable = false;
        }

        private void Update()
        {
            if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.NOT_START)
            {
                _processingText.text = "not started";
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.STARTED)
            {
                _processingText.text = "started";
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.STOPPED)
            {
                _processingText.text = "stopped";
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.FINISH)
            {
                _processingText.text = "finished";
            }
        }

        public void PlayVideo()
        {
            // Set root folder.
            isPlayVideo = true;
            VideoPlayer.instance.SetRootFolder();
            // Play capture video.
            VideoPlayer.instance.PlayVideo();
        }

        private void Next()
        {
            switch (VideoCaptureCtrl.instance.status)
            {
                case  VideoCaptureCtrl.StatusType.NOT_START:
                    break;
                case VideoCaptureCtrl.StatusType.STARTED:
                    break;
                case VideoCaptureCtrl.StatusType.STOPPED:
                    break;
            }
        }

    }
}