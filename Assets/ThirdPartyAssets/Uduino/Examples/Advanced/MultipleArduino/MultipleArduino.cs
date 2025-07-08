using UnityEngine;
using System.Collections;
using Uduino;

public class MultipleArduino : MonoBehaviour
{
    // In this example, you get the connected in the device loop.
    // see MultipleArduino2 for another way to do it.
    UduinoManager u;
    int sensorOne = 0;
    int sensorTwo = 0;

    void Start()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived;
    }

    void Update()
    {
        if (UduinoManager.Instance.hasBoardConnected())
        {
            UduinoDevice firstDevice = UduinoManager.Instance.GetBoard("firstArduino");
            UduinoDevice secondDevice = UduinoManager.Instance.GetBoard("secondArduino");
            UduinoManager.Instance.sendCommand(firstDevice, "GetVariable");
            UduinoManager.Instance.sendCommand(secondDevice, "GetVariable");
            Debug.Log("Variable of the first board:" + sensorOne);
            Debug.Log("Variable of the second board:" + sensorTwo);
        } else
        {
            Debug.Log("The boards have not been detected");
        }
    }

    void OnDataReceived(string data, UduinoDevice device)
    {
        if (device.name == "firstArduino") sensorOne = int.Parse(data);
        else if (device.name == "secondArduino") sensorTwo = int.Parse(data);
    }
}
