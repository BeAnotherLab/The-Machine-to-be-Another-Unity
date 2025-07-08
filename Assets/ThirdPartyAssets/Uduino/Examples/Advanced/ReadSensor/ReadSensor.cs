using UnityEngine;
using System.Collections;
using Uduino;

public class ReadSensor : MonoBehaviour
{

    UduinoManager u;

    void Awake()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived; //Create the Delegate
    }

    void Update()
    {
        UduinoDevice myDevice = UduinoManager.Instance.GetBoard("myArduinoName");
        UduinoManager.Instance.Read(myDevice, "mySensor"); // Read every frame the value of the "mySensor" function on our board. 
    }

    void OnDataReceived(string data, UduinoDevice device)
    {
        Debug.Log(data); // Use the data as you want !
    }

    void Read()
    {

    }
}