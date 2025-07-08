using UnityEngine;
using System.Collections;
using Uduino; // adding Uduino NameSpace 

public class MultipleButtons : MonoBehaviour
{

    public class UduinoButton
    {
        int _pin;
        public GameObject _buttonGameObject;
        int buttonValue = 0;
        int prevButtonValue = 0;

        public UduinoButton(int pin, GameObject gameObject)
        {
            _pin = pin;
            _buttonGameObject = gameObject;
            UduinoManager.Instance.pinMode(pin, PinMode.Input_pullup);
        }

        public int Read() {
            buttonValue = UduinoManager.Instance.digitalRead(_pin);
            return buttonValue;
        }

        public bool pressedDown() {
            if (valueChanged() && buttonValue == 0)
            {
                prevButtonValue = buttonValue;
                return true;
            }
            return false;
        }

        public bool pressedUp() {
            if (valueChanged() && buttonValue == 1)
            {
                prevButtonValue = buttonValue;
                return true;
            }
            return false;
        }

        bool valueChanged() {
            if (buttonValue != prevButtonValue)
                return true;
            return false;
        }

    }

    UduinoManager u; // The instance of Uduino is initialized here
    public int buttonOnePin = 9;
    public int buttonTwoPin = 10;
    public int buttonThreePin = 11;
    public GameObject buttonOneGameObject;
    public GameObject buttonTwoGameObject;
    public GameObject buttonThreeGameObject;
    UduinoButton buttonOne;
    UduinoButton buttonTwo;
    UduinoButton buttonThree;

    void Start()
    {
        buttonOne = new UduinoButton(buttonOnePin, buttonOneGameObject);
        buttonTwo = new UduinoButton(buttonTwoPin, buttonTwoGameObject);
        buttonThree = new UduinoButton(buttonThreePin, buttonThreeGameObject);
        UduinoManager.Instance.OnDataReceived += DataReceived;
    }

    void DataReceived(string value, UduinoDevice board) {
        //  Debug.Log(value); //Used for debugging purpose
    }


    private void Update()
    {
        buttonOne.Read();
        buttonTwo.Read();
        buttonThree.Read();

    
        if (buttonOne.pressedDown())
            PressedDown(buttonOne);
        else if (buttonOne.pressedUp())
            PressedUp(buttonOne);

        if (buttonTwo.pressedDown())
            PressedDown(buttonTwo);
        else if (buttonTwo.pressedUp())
            PressedUp(buttonTwo);


        if (buttonThree.pressedDown())
            PressedDown(buttonThree);
        else if (buttonThree.pressedUp())
            PressedUp(buttonThree);
    }

    void PressedDown(UduinoButton button)
    {
        button._buttonGameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
        button._buttonGameObject.transform.GetChild(0).Translate(Vector3.down / 20);
    }

    void PressedUp(UduinoButton button)
    {
        button._buttonGameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
        button._buttonGameObject.transform.GetChild(0).Translate(Vector3.up / 20);
    }
}