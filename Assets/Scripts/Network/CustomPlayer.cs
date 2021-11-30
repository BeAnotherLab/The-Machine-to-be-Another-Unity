using System;
using System.Collections.Generic;
using System.IO;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class CustomPlayer : NetworkBehaviour
    {
        public bool consentGiven;

        private GameObject _mainCamera;
        private GameObject _videoFeedFlipParent;
        //[SerializeField] private BoolVariable _consentGiven;
        [SerializeField] private StringVariable _userID;
        [SerializeField] private BoolGameEvent _consentAnswerGivenEvent;
        [SerializeField] private BoolGameEvent _readyToShowQuestionnaire;
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
        void Update()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
            {
                transform.rotation = _mainCamera.transform.rotation;
                GetComponentInChildren<MeshRenderer>().enabled = false;
            }
        }

        public void ConsentButtonPressed(bool answer)
        {
            if (!isLocalPlayer)
                return;
            CmdGiveConsent("id", answer);            
        }
        
        [Command]
        public void CmdGiveConsent(string id, bool answer)
        {
            consentGiven = answer;
            _consentAnswerGivenEvent.Raise(answer);
        }
        
        [ClientRpc]
        public void RpcBothConsentGiven(bool consent)
        {
            if (isLocalPlayer) return;
            _readyToShowQuestionnaire.Raise(consent);
            if (consent) Debug.Log("both consent given, showing questionaire");
            if (!consent) Debug.Log("one user refused, NOT showing questionaire!");
        }

    }
}