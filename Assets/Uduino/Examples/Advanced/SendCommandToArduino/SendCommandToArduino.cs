using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class SendCommandToArduino : MonoBehaviour {

    [Range(0,255)]
    public int val1 = 150;
    [Range(0, 255)]
    public int val2 = 50;
    [Range(0, 255)]
    public int val3 = 255;

    // Use this for initialization
    void Start () {
        UduinoManager.Instance.OnDataReceived += ReceviedData;
        StartCoroutine(Loop()); // Reduce the send rate
    }

    IEnumerator Loop()
    {
        while (true)
        {
            UduinoManager.Instance.sendCommand("command", val1, val2, val3);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ReceviedData(string data, UduinoDevice device)
    {
        Debug.Log(data);
    }
}
