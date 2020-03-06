using UnityEngine;
using System.Collections;
using Uduino;

public class ButtonTrigger : MonoBehaviour
{
    public GameObject button;
    UduinoManager u;


    // Solution 1:

    void Awake()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived; //Create the Delegate
        UduinoManager.Instance.alwaysRead = true; // This value should be On By Default
    }

    void OnDataReceived(string data, UduinoDevice deviceName)
    {
        if (data == "1")
            PressedDown();
        else if (data == "0")
            PressedUp();
    }

    // Solution 2 
    
    /*
    void Awake()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected; //Create the Delegate
    }

    void OnBoardConnected(UduinoDevice connectedDevice)
    {
        connectedDevice.callback = ButtonTriggerEvt;
    }

    void ButtonTriggerEvt(string data)
    {
        if (data == "1")
            PressedDown();
        else if (data == "0")
            PressedUp();
    }
    */



    void PressedDown()
    {
        button.GetComponent<Renderer>().material.color = Color.red;
        button.transform.Translate(Vector3.down / 10);
    }

    void PressedUp()
    {
        button.GetComponent<Renderer>().material.color = Color.green;
        button.transform.Translate(Vector3.up / 10);
    }
}