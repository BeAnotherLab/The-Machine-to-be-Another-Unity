﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RockVR.Video
{
    public class CustomVideoPlayer : MonoBehaviour
    {
        /// <summary>
        /// Save the video files.
        /// </summary>
        public List<string> videoFiles = new List<string>();
      
        public static CustomVideoPlayer instance;
        
        public delegate void VideoFinished();
        public static event VideoFinished OnVideoFinished;
       
        /// <summary>
        /// Play video properties.
        /// </summary>
        private UnityEngine.Video.VideoPlayer videoPlayerImpl;
        
        private void Awake()
        {
            //TODO this should no longer be necessary?
            if (instance == null) instance = this;
            videoPlayerImpl = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayerImpl.loopPointReached += delegate(UnityEngine.Video.VideoPlayer source) { VideoFeed.instance.ShowLiveFeed(true); };
        }

        /// <summary>
        /// Add video file to video file list.
        /// </summary>
        public void SetRootFolder()
        {
            if (Directory.Exists(PathConfig.SaveFolder))
            {
                DirectoryInfo direction = new DirectoryInfo(PathConfig.SaveFolder);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                videoFiles.Clear();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".mp4"))
                    {
                        videoFiles.Add(PathConfig.SaveFolder + files[i].Name);
                    }
                }
            }
            // Init VideoPlayer properties.
            videoPlayerImpl.enabled = true;
            videoPlayerImpl.targetCamera = Camera.main;
            videoPlayerImpl.loopPointReached += delegate(UnityEngine.Video.VideoPlayer source) { FinishVideo(); };
            if (gameObject.GetComponent<AudioSource>() != null)
            {
                videoPlayerImpl.SetTargetAudioSource(0, gameObject.GetComponent<AudioSource>());
                gameObject.GetComponent<AudioSource>().clip = null;
            }
        }
        /// <summary>
        /// Play video process.
        /// </summary>
        public void PlayVideo()
        {
            VideoCameraManager.instance.ShowRecordedVideoForUser();
            GetComponent<UnityEngine.Video.VideoPlayer>().url = "file://" + videoFiles.Last();
            Debug.Log("[VideoPlayer::PlayVideo] Video Path:" + videoFiles.Last());
            videoPlayerImpl.Play();
        }
        
        public void StopVideo()
        {
            videoPlayerImpl.Stop();
        }

        private void FinishVideo()
        {
            videoPlayerImpl.enabled = false;
            OnVideoFinished();
        }
    }
}
