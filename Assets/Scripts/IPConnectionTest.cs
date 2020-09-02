using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPConnectionTest : MonoBehaviour {

    private TCPClient tcpClient;
    public Button connectionButton;

    // Use this for initialization
    void Awake () {
        tcpClient = FindObjectOfType<TCPClient>();
	}

    public void SetConnection()
    {
        tcpClient.ip = "localhost";
        tcpClient.ConnectToTcpServer();
        StartCoroutine(WaitForServerStatus());
    }

    private IEnumerator WaitForServerStatus() //Not the most elegant way
    {
        yield return new WaitForSeconds(1f);//waits for one second to check for server status.

        if (tcpClient.isConnected)
        {
            connectionButton.GetComponent<Image>().color = Color.green;
            tcpClient.SendTCPMessage("test connection message");
        }

        else
            connectionButton.GetComponent<Image>().color = Color.red;
    }
}
