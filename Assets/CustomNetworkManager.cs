﻿using UnityEngine;

namespace Mirror.Examples.Pong
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class CustomNetworkManager : NetworkManager
    {
        public Transform leftRacketSpawn;
        public Transform rightRacketSpawn;

        private GameObject _mainCamera;
        
        private void Awake()
        {
            _mainCamera = GameObject.Find("Main Camera");
        }
        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            Transform start = numPlayers == 0 ? leftRacketSpawn : rightRacketSpawn;
            GameObject player = Instantiate(playerPrefab, start.position, start.rotation, start);
            player.gameObject.name = "Player" + numPlayers + 1;
            _mainCamera.transform.SetParent(start, false);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }
    }
}