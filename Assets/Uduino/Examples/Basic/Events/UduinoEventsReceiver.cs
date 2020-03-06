using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class UduinoEventsReceiver : MonoBehaviour {


	public void DataReceived (string message, UduinoDevice device) {
        Debug.Log("Event: Message :\"" + message + "\" , received from board " + device.name);
	}

    public void BoardConnected(UduinoDevice device)
    {
        Debug.Log("Event: Board " + device.name +" connected");
    }

    public void BoardDisconnected(UduinoDevice device)
    {
        Debug.Log("Event: Board " + device.name + " disconnected.");
    }
}
