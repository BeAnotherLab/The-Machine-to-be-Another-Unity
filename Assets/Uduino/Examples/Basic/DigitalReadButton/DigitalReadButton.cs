using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class DigitalReadButton : MonoBehaviour {

    public int button = 9;
    public GameObject buttonGameObject;

    int buttonValue = 0;
    int prevButtonValue = 0;

    void Start ()
    {
        UduinoManager.Instance.pinMode(button, PinMode.Input_pullup);
    }

    void Update ()
    {
        buttonValue = UduinoManager.Instance.digitalRead(button);

        // In this case, we compare the current button value to the previous button value, 
        // to trigger the change only once the value change.
        if (buttonValue != prevButtonValue)
        {
            if (buttonValue == 0)
            {
                PressedDown();
            }
            else if (buttonValue == 1)
            {
                PressedUp();
            }
            prevButtonValue = buttonValue; // Here we assign prev button value to the new value
        }

    }

    void PressedDown()
    {
        buttonGameObject.GetComponent<Renderer>().material.color = Color.red;
        buttonGameObject.transform.Translate(Vector3.down / 20);
    }

    void PressedUp()
    {
        buttonGameObject.GetComponent<Renderer>().material.color = Color.green;
        buttonGameObject.transform.Translate(Vector3.up / 20);
    }

}
