using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = DebugFile;

namespace Mirror.Examples.Pong
{
    [AddComponentMenu("")]
    
    public class CustomNetworkManager : NetworkManager
    {
        public bool offlineMode; //TODO remove?;
        
        public delegate void OnConnectionEstablished();
        public static OnConnectionEstablished ConnectionEstablished;
        
        private void OnEnable()
        {
            DisplayManager.SetDisplayModeEvent += EnableNetworkGUI;
        }

        private void OnDisable()
        {
            DisplayManager.SetDisplayModeEvent -= EnableNetworkGUI;
        }

        private void Start()    
        {
            if (offlineMode) Instantiate(playerPrefab); //TODO needed?
            networkAddress = PlayerPrefs.GetString("othersIP");

            if (PlayerPrefs.GetInt("repeater", 0) == 1) //TODO rename property
                StartHost();
            else
                StartCoroutine(TryConnect());
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // add player at correct spawn position
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
            ConnectionEstablished();
        }

        public void EnableNetworkGUI(bool show)
        {
            //GetComponent<NetworkManagerHUD>().showGUI = show;
        }         
        
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
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