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
        [SerializeField] private bool _meshRendererEnabled;
        
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
            Debug.Log(gameObject.name);
            
            //if novrbutton pressed, show local player
            if (isLocalPlayer)
            {
                if (show)
                    Debug.Log("received No VR in local player, enabling mesh renderer");
                else 
                    Debug.Log("received VR on in local player, disabling mesh renderer");
                //_meshRenderer.enabled = show;
                GetComponent<NetworkTransform>().enabled = show;
                _meshRendererEnabled = show;
                if (show == false)//if we disabled the network transform
                {
                    transform.rotation = Quaternion.Euler(0,0,0);
                }
            }
            //if vrbutton pressed, show remote player
            else if (!isLocalPlayer)
            {
                if (show)
                    Debug.Log("received No VR in remote player, disabling mesh renderer");
                else 
                    Debug.Log("received VR on in remote player, enabling mesh renderer");
                
                Debug.Log("received No VR in local player, enabling mesh renderer");
                //_meshRenderer.enabled = !show;
                GetComponent<NetworkTransform>().enabled = !show;
                if (show == true) //if we disabled the network transform
                {
                    transform.rotation = Quaternion.Euler(0,0,0);
                }
                _meshRendererEnabled = !show;
            }
        }
    }
}