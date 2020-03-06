using UnityEngine;
using System.Collections;
using Uduino;

public class ServoOptimized : MonoBehaviour {

    [Range(0, 180)]
    public int servoAngle = 0;
    private int prevServoAngle = 0;

    void Update()
    {
        OptimizedWrite();
    }

    void OptimizedWrite()
    {
        if (servoAngle != prevServoAngle) // Use this condition to not write at each frame 
        {
            UduinoManager.Instance.sendCommand("R", servoAngle);
            prevServoAngle = servoAngle;
            /* // Note : If you have several board connected, you can specify to send the information to a specific board by using: 
             UduinoDevice targetDevice = UduinoManager.Instance.GetBoard("servoBoard"); // "servoBoard" is the name defined at the top of your Arduino sketch
             UduinoManager.Instance.sendCommand(targetDevice,"R", servoAngle);
             */
        }
    }
}
