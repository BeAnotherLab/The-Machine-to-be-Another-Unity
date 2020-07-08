using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.Basic
{
    public class CustomPlayer: NetworkBehaviour
    {
        public string playerData;
        
        // These are set in OnStartServer and used in OnStartClient
        [SyncVar]
        public int playerNo;
        [SyncVar]
        public Color playerColor;

        // This is updated by UpdateData which is called from OnStartServer via InvokeRepeating
        [SyncVar(hook = nameof(OnPlayerDataChanged))]
        public int playerDataInt;

        // This is called by the hook of playerData SyncVar above
        void OnPlayerDataChanged(int oldPlayerData, int newPlayerData)
        {
            // Show the data in the UI
            playerData = string.Format("Data: {0:000}", newPlayerData);
        }

        // This fires on server when this player object is network-ready
        public override void OnStartServer()
        {
            base.OnStartServer();

            // Set SyncVar values
            playerNo = connectionToClient.connectionId;
            playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);

            // Start generating updates
            InvokeRepeating(nameof(UpdateData), 1, 1);
        }

        // This only runs on the server, called from OnStartServer via InvokeRepeating
        [ServerCallback]
        void UpdateData()
        {
            playerDataInt = Random.Range(100, 1000);
        }

        // This fires on all clients when this player object is network-ready
        public override void OnStartClient()
        {
            base.OnStartClient();

            // Make this a child of the layout panel in the Canvas
            transform.SetParent(GameObject.Find("VideoFeedFlipParent").transform);

        }

        // This only fires on the local client when this player object is network-ready
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
        }
    }
}
