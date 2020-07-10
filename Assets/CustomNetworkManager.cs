using UnityEngine;

namespace Mirror.Examples.Pong
{
    // Custom NetworkManager that simply assigns the correct racket positions when
    // spawning players. The built in RoundRobin spawn method wouldn't work after
    // someone reconnects (both players would be on the same side).
    [AddComponentMenu("")]
    public class CustomNetworkManager : NetworkManager
    {

        public static CustomNetworkManager instance;

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void Start()
        {
            networkAddress = PlayerPrefs.GetString("othersIP");
            
            if (PlayerPrefs.GetInt("serialControlOn", 0) == 1) //TODO rename property
                StartHost();
            else 
                StartClient();
        }
        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            // add player at correct spawn position
            GameObject player = Instantiate(playerPrefab);
            NetworkServer.AddPlayerForConnection(conn, player);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
        }

    }
}