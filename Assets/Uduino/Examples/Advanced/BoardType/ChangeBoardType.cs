using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;

public class ChangeBoardType : MonoBehaviour
{

    int customPinAnalog = 0;
    int customPinDigital = 0;

    void Start()
    {
        UduinoManager.Instance.OnBoardConnected += OnBoardConnected; 
    }

    void OnBoardConnected(UduinoDevice connectedDevice)
    {
        // If you have one baord connected 
        UduinoManager.Instance.SetBoardType(connectedDevice,"Arduino Mega");
        //Get the pin for a custom board
        customPinAnalog = UduinoManager.Instance.GetPinNumberFromBoardType("Arduino Mega", "A14");
        // If the board is already set with SetBoardType, you can get the Pin iD by usong
        customPinAnalog = UduinoManager.Instance.GetPinFromBoard("A14");

        UduinoManager.Instance.pinMode(customPinAnalog, PinMode.Input);
        Debug.Log("The pin A14 pinout for Arduino Mega is " + customPinAnalog);

        //Get the pin for a custom board
        customPinDigital = BoardsTypeList.Boards.GetBoardFromName("Arduino Mega").GetPin("42"); // returns 42
        UduinoManager.Instance.pinMode(customPinDigital, PinMode.Output);
    }

}
