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
        [SerializeField] private  BoolGameEvent bothConsentGiven;
            
        public bool offlineMode;
        private int _consentCount;
        [SerializeField] private IntVariable _consentsGiven;
        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()    
        {
            if (offlineMode) Instantiate(playerPrefab);

            networkAddress = PlayerPrefs.GetString("othersIP");

            if (PlayerPrefs.GetInt("repeater", 0) == 1) //TODO rename property
                StartHost();
            else
                StartCoroutine(TryConnect());
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
                if (_consentsGiven.Value == 2)
                {
                    bothConsentGiven.Raise(true);
                }
                else
                {
                    bothConsentGiven.Raise(false);
                }
            }
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