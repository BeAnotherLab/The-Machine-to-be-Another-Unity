using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class MegaReadWrite : MonoBehaviour {

    // Be sure to select Arduino MEGA as board type
    // If your board is not detected, increase Discover delay to 1.5s or more. 
    int pin14;
    int digitalPin42;

    void Start () {

        // Set pin mode when the board is detected. Getting the value when the board is connected
        UduinoManager.Instance.OnBoardConnected += BoardConnected;


        // You can setup Arduino type by code but I recommand to set the arduino board type with the inspector panel. 
        //UduinoManager.Instance.SetBoardType("Arduino Mega"); //If you have one board connected
        //digitalPin42 = BoardsTypeList.Boards.GetPin("42"); // returns 42
        // UduinoManager.Instance.pinMode(digitalPin42, PinMode.Output);
        // Debug.Log("The pin 42 pinout for Arduino Mega is " + pin14);
    }

    void BoardConnected(UduinoDevice device)
    {
    //    if(UduinoManager.Instance.isConnected()) // implemented in the next version. Safe to remove 
        {
            pin14 = UduinoManager.Instance.GetPinFromBoard("A14");
            UduinoManager.Instance.pinMode(pin14, PinMode.Input);
            Debug.Log("The pin A14 pinout for Arduino Mega is " + pin14);


            digitalPin42 = UduinoManager.Instance.GetPinFromBoard("42");
            UduinoManager.Instance.pinMode(digitalPin42, PinMode.Output);
            Debug.Log("The pin 42 pinout for Arduino Mega is " + digitalPin42);
        }

    }

    private void Update()
    {
        int value = UduinoManager.Instance.analogRead(pin14);
        if(value != -1)
         Debug.Log("Analog Value : " + value);
        UduinoManager.Instance.digitalWrite(digitalPin42, State.HIGH);
    }
}
