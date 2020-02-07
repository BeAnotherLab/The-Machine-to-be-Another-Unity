using System;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;

namespace RockVR.Video.Demo
{
    public class CustomVideoCaptureUI : MonoBehaviour
    {
        [SerializeField] private Button _recordButton;
        [SerializeField] private Button _stopRecordingButton;
        [SerializeField] private Button _playVideosButton;
        [SerializeField] private Button _nextVideoButton;

        private bool isPlayVideo = false;
        private void Awake()
        {
            Application.runInBackground = true;
            isPlayVideo = false;
            _recordButton.onClick.AddListener( () => VideoCaptureCtrl.instance.StartCapture());
            _stopRecordingButton.onClick.AddListener(() => VideoCaptureCtrl.instance.StopCapture());
            _playVideosButton.onClick.AddListener(() => VideoPlayer.instance.PlayVideo());
            _nextVideoButton.onClick.AddListener(() => VideoPlayer.instance.NextVideo());
        }

        private void Start()
        {
            _stopRecordingButton.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.NOT_START)
            {
                _stopRecordingButton.gameObject.SetActive(false);
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.STARTED)
            {
                _playVideosButton.gameObject.SetActive(false);
                if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
                {
                    VideoCaptureCtrl.instance.StopCapture();
                }
                if (GUI.Button(new Rect(180, Screen.height - 60, 150, 50), "Pause Capture"))
                {
                    VideoCaptureCtrl.instance.ToggleCapture();
                }
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.PAUSED)
            {
                if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Stop Capture"))
                {
                    VideoCaptureCtrl.instance.StopCapture();
                }
                if (GUI.Button(new Rect(180, Screen.height - 60, 150, 50), "Continue Capture"))
                {
                    VideoCaptureCtrl.instance.ToggleCapture();
                }
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.STOPPED)
            {
                if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Processing"))
                {
                    // Waiting processing end.
                }
            }
            else if (VideoCaptureCtrl.instance.status == VideoCaptureCtrl.StatusType.FINISH)
            {
                if (!isPlayVideo)
                {
                    if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "View Video"))
                    {
#if UNITY_5_6_OR_NEWER
                        // Set root folder.
                        isPlayVideo = true;
                        VideoPlayer.instance.SetRootFolder();
                        // Play capture video.
                        VideoPlayer.instance.PlayVideo();
                    }

                    if (GUI.Button(new Rect(180, Screen.height - 60, 150, 50), "Start Capture"))
                    {
                        VideoCaptureCtrl.instance.StartCapture();                        
                    }

                }
                else
                {
                    if (GUI.Button(new Rect(10, Screen.height - 60, 150, 50), "Next Video"))
                    {
                        // Turn to next video.
                        VideoPlayer.instance.NextVideo();
                        // Play capture video.
                        VideoPlayer.instance.PlayVideo();
#else
                        // Open video save directory.
                        Process.Start(PathConfig.saveFolder);
#endif
                    }

                    if (GUI.Button(new Rect(180, Screen.height - 60, 150, 50), "Start Capture"))
                    {
                        VideoCaptureCtrl.instance.StartCapture();
                        VideoPlayer.instance.StopVideo();
                    }
                }
            }
        }
    }
}