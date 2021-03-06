﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPConnectionTest : MonoBehaviour {

    private TCPClient tcpClient;
    public Button connectionButton;
    public string ip;

    // Use this for initialization
    void Awake () {
        tcpClient = FindObjectOfType<TCPClient>();
        connectionButton.onClick.AddListener(delegate { SetConnection(); });
        tcpClient.ip = ip;
    }

    public void SetConnection()
    {
        tcpClient.ConnectToTcpServer();
        StartCoroutine(WaitForServerStatus());
    }

    private IEnumerator WaitForServerStatus() //Not the most elegant way
    {
        yield return new WaitForSeconds(1f);//waits for one second to check for server status.

        if (tcpClient.isConnected)
        {
            connectionButton.GetComponent<Image>().color = Color.green;
            tcpClient.SendTCPMessage("test_connection_message");
        }

        else
            connectionButton.GetComponent<Image>().color = Color.red;
    }
}
