﻿using System;
using System.Collections.Generic;
using System.IO;
using ScriptableObjectArchitecture;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class CustomPlayer : NetworkBehaviour
    {
        public BoolVariable consentGiven;

        private GameObject _mainCamera;
        private GameObject _videoFeedFlipParent;
        //[SerializeField] private BoolVariable _consentGiven;
        
        [SerializeField] [SyncVar (hook = nameof(SetPairID))] private string _pairId;
        [SerializeField] private BoolGameEvent _consentAnswerGivenEvent;
        [SerializeField] private BoolGameEvent _readyToShowQuestionnaire;
        [SerializeField] private ResponseData _responseData;
        
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

            if (_pairId == "") //pair ID is empty if it has not been set yet this session, generate a new one
                CmdGiveConsent(answer, Guid.NewGuid().ToString());
            else //we already have a pair ID, no need for another one
                CmdGiveConsent(answer, "");
        }
        
        private void SetPairID(string oldpairId, string newPairId){
            _responseData.pairID = newPairId;
            _pairId = newPairId;
        }
        
        [Command] //Commands are sent from player objects on the client to player objects on the server. 
        public void CmdGiveConsent(bool answer, string pairId)
        {
            if(pairId != "") _pairId = pairId; //assign the syncvar
            
            _consentAnswerGivenEvent.Raise(answer);
        }
        
        [ClientRpc] //ClientRpc calls are sent from objects on the server to objects on clients. 
        public void RpcBothConsentGiven(bool consent)
        {
            if (isLocalPlayer) return;
            _readyToShowQuestionnaire.Raise(consent);
            if (consent) Debug.Log("both consent given, showing questionaire");
            if (!consent) Debug.Log("one user refused, NOT showing questionaire!");
        }

    }
}