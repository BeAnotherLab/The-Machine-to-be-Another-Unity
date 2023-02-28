using System;
using System.Collections.Generic;
using System.IO;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class CustomPlayer : NetworkBehaviour
    {
        private GameObject _mainCamera;
        private GameObject _videoFeedFlipParent;

        [SerializeField] private MeshRenderer _meshRenderer;
        
        private void Awake()
        {
            _mainCamera = GameObject.Find("Main Camera");
            _videoFeedFlipParent = GameObject.Find("VideoFeedFlipParent");
        }

        private void Start()
        {
            if (!isLocalPlayer)
            {
                transform.SetParent(_videoFeedFlipParent.transform, false);
                VideoFeed.instance.targetTransform = transform;
                gameObject.name = "remote player";
            }
            else
            {
                gameObject.name = "local player";
            }
            StartCoroutine(VideoFeed.instance.StartupDim());
            transform.GetChild(0).transform.localRotation = Quaternion.Euler(0,0, PlayerPrefs.GetFloat("tiltAngle"));
        }

        // need to use FixedUpdate for rigidbody
        private void Update()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer) //TODO harness this for VR/NoVR mode
            {
                transform.rotation = _mainCamera.transform.rotation;
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        public void noVRButtonPressed(bool show)
        {
            //if novrbutton pressed, show local player
            if (isLocalPlayer)
            {
                _meshRenderer.enabled = show;    
            }
            //if vrbutton pressed, show remote player
            else if (!isLocalPlayer)
            {
                _meshRenderer.enabled = !show;
            }
        }
    }
}