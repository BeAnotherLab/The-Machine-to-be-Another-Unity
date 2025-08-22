using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class CustomPlayer : NetworkBehaviour
    {
        public delegate void OnSignalingSelf(Transform selfTransform);
        public static OnSignalingSelf SignalingSelf;

        [SerializeField] private BoolGameEvent _dimGameEvent;
        [SerializeField] private GameObject _pano;
        
        private GameObject _mainCamera;
        private GameObject _videoFeedFlipParent;

        [SerializeField] private bool _loadTiltFromPlayerPrefs = true;

        [SerializeField] private float _tiltAngle;


        private void OnEnable()
        {
            SettingsGUI.RotateCamera += Rotate;
        }

        private void OnDisable()
        {
            SettingsGUI.RotateCamera -= Rotate;
        }

        private void Awake()
        {
            _mainCamera = GameObject.Find("Main Camera");
            _videoFeedFlipParent = GameObject.Find("VideoFeedFlipParent");
        }

        private void Start()
        {           
            if (_loadTiltFromPlayerPrefs) _tiltAngle = PlayerPrefs.GetFloat("tiltAngle");

            if (!isLocalPlayer)
            {
                transform.SetParent(_videoFeedFlipParent.transform, false);
                SignalingSelf(gameObject.transform);
                gameObject.name = "remote player";
            }
            else
            {
                gameObject.name = "local player";
            }
            transform.GetChild(0).transform.localRotation = Quaternion.Euler(0,0, PlayerPrefs.GetFloat("tiltAngle"));
            StartCoroutine(StartupDim());
        }

        // need to use FixedUpdate for rigidbody
        private void Update()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
            {
                transform.rotation = _mainCamera.transform.rotation;
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
            
            if (Input.GetKeyDown("r")) Rotate();

        }
        
        private IEnumerator StartupDim() 
        {
            yield return new WaitForSeconds(2);
            _dimGameEvent.Raise(true);
        }

        private void Rotate()
        {
            _tiltAngle += 90;
            _pano.transform.eulerAngles = new Vector3(0, 0, _tiltAngle);
            PlayerPrefs.SetFloat("tiltAngle", _tiltAngle);
        }

    }
}