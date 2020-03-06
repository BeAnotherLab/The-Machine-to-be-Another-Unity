using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class SimpleUduino : MonoBehaviour {

    int ledPin = 11;

    int servoPin = 9;
    [Range(0,180)]
    public int servoValue = 0;

    int sensorPin = (int)AnalogPin.A0;
    int sensorValue= 0;

    void Start () {
        UduinoManager.Instance.pinMode(ledPin, PinMode.PWM);
        UduinoManager.Instance.pinMode(servoPin, PinMode.Servo);
        UduinoManager.Instance.pinMode(sensorPin, PinMode.Input);
    }

    void Update () {
        UduinoManager.Instance.Read(sensorPin, (string s) => sensorValue = int.Parse(s));
        Debug.Log(sensorValue);
    }
}
