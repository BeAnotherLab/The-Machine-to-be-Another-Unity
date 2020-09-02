// adapted from from Daniel Bierwirth https://gist.github.com/danielbierwirth/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    public bool sustainConnectionAcrossScenes;
    public string ip;
    [HideInInspector]
    public bool isConnected;

    public static TCPClient instance;
    
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    #endregion

    void Awake()
    {
        if (instance == null) instance = this;
        if(sustainConnectionAcrossScenes) { 
            GameObject thisGameObject = this.gameObject;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    
    public void ConnectToTcpServer()
    {
        try {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }


    /// Runs in background clientReceiveThread; Listens for incomming data. 	
    private void ListenForData()
    {
        try {
            socketConnection = new TcpClient(ip, 4012);
            Byte[] bytes = new Byte[1024];
            while (true) {
                if(!isConnected) isConnected = true; //perhaps not the most elegant way to report if its connected
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream()) {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("server message received as: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
            if (isConnected) isConnected = false; //perhaps not the most elegant way to report if its connected
        }
    }

    /// Send message to server using socket connection. 	
    public void SendTCPMessage(string clientMessage) {
        if (socketConnection == null) return;

        try {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                //string clientMessage = "This is a message from one of your clients.";
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
               // Debug.Log("Client sent this message. " + clientMessage + " should be received by server");
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

}