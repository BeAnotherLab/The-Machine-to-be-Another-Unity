using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPConnectionTest : MonoBehaviour {

    public Text inputIP, placeHolderIP;
    private TCPClient tcpClient;
    public Button connectionButton;

    // Use this for initialization
    void Awake () {
        tcpClient = FindObjectOfType<TCPClient>();
	}

    private void Start()
    {
        placeHolderIP.text = PlayerPrefs.GetString("server_ip");
    }

    public void SetConnection()
    {
        if (inputIP.text != "") PlayerPrefs.SetString("server_ip", inputIP.text);
            

        tcpClient.ip = PlayerPrefs.GetString("server_ip");

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
