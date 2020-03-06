using UnityEngine;
using System.Collections;
using Uduino;

public class MultipleArduino2 : MonoBehaviour
{
    UduinoManager u;
    int sensorOne = 0;
    int sensorTwo = 0;

    UduinoDevice firstDevice = null;
    UduinoDevice secondDevice = null;

    void Start()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected;
        UduinoManager.Instance.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        if(firstDevice != null)
            UduinoManager.Instance.sendCommand(firstDevice, "GetVariable");
        if (secondDevice != null)
            UduinoManager.Instance.sendCommand(secondDevice, "GetVariable");
        Debug.Log("Variable of the first board:" + sensorOne);
        Debug.Log("Variable of the second board:" + sensorTwo);
    }

    void OnDataReceived(string data, UduinoDevice device)
    {
        if (device.name == "firstArduino") sensorOne = int.Parse(data);
        else if (device.name == "secondArduino") sensorTwo = int.Parse(data);
    }

    // Different setups for each arduino board
    void OnBoardConnected(UduinoDevice connectedDevice)
    {
        //You can launch specific functions here
        if (connectedDevice.name == "firstArduino")
        {
            firstDevice = connectedDevice;
        }
        else if (connectedDevice.name == "secondArduino")
        {
            secondDevice = connectedDevice;
        }
    }
}
