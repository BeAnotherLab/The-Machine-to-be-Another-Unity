using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = DebugFile;

namespace Mirror.Examples.Pong
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    
    public class CustomNetworkManager : NetworkManager
    {
        public static CustomNetworkManager instance;
            
        public bool offlineMode;
        
        //Data collection consent values and events 
        [SerializeField] private BoolGameEvent bothConsentGiven; //TODO move to a simple int field in this class
        private int _consentCount; //how many times consent answers were given
        [SerializeField] private IntVariable _consentsGiven; //how many positive answers were given

        //Pre/Post questionnaires end events 
        [SerializeField] private QuestionnaireStateGameEvent _bothQuestionnairesFinishedRpcEvent;
        private int _questionnairesPreFinishedCount;
        private int _questionnairesPostFinishedCount;

        //video data collection consent
        [SerializeField] private IntVariable _videoConsentsGiven; //how many positive answers were given TODO move to a simple int field
        
        [SerializeField] private BoolVariable _sendRecordingCommand;

        private void OnEnable()
        {
            DisplayManager.SetDisplayModeEvent += EnableNetworkGUI;
        }

        private void OnDisable()
        {
            DisplayManager.SetDisplayModeEvent -= EnableNetworkGUI;
        }

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()    
        {
            if (offlineMode) Instantiate(playerPrefab);
            _consentsGiven.Value = 0;
            _videoConsentsGiven.Value = 0; //In standby instead?
            networkAddress = PlayerPrefs.GetString("othersIP");

            if (PlayerPrefs.GetInt("repeater", 0) == 1) //TODO rename property
                StartHost();
            else
                StartCoroutine(TryConnect());
        }

        public void OnStandby()
        {
            if (_consentsGiven.Value > 0) _consentsGiven.Value = 0;
            if (_videoConsentsGiven.Value > 0) _videoConsentsGiven.Value = 0;
            _sendRecordingCommand.Value = false;
            if (_consentCount > 0) _consentCount = 0;
            _questionnairesPreFinishedCount = 0;
            _questionnairesPostFinishedCount = 0;
        }
        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public void EnableNetworkGUI(bool show)
        {
            GetComponent<NetworkManagerHUD>().showGUI = show;
        }         
        
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }

        public void ConsentAnswerGiven(bool consent)
        {
            _consentCount++;
            if (consent) _consentsGiven.Value++;
            if (_consentCount == 2)
            {
                if (_consentsGiven.Value == 2) bothConsentGiven.Raise(true);
                else bothConsentGiven.Raise(false);
            }
        }
        
        public void QuestionnaireFinished(QuestionnaireState state)
        {
            if (state == QuestionnaireState.pre)
            {
                _questionnairesPreFinishedCount++;
                if (_questionnairesPreFinishedCount == 2)
                {
                    _bothQuestionnairesFinishedRpcEvent.Raise(QuestionnaireState.pre);
                }
            }
            else if (state == QuestionnaireState.post)
            {
                _questionnairesPostFinishedCount++;
                if (_questionnairesPostFinishedCount == 2)
                {
                    _bothQuestionnairesFinishedRpcEvent.Raise(QuestionnaireState.post);
                }
            }
        }
        
        private void VideoConsentAnswerGiven(bool consent)
        {
            if (consent) _videoConsentsGiven.Value++;
            _sendRecordingCommand.Value = _videoConsentsGiven.Value == 2;
        }
        
        private IEnumerator TryConnect()
        {
            while (!NetworkClient.isConnected)
            {
                Debug.Log("trying to connect to host.");
                StartClient();
                yield return new WaitForSeconds(4);
            }
        }
        
    }
}