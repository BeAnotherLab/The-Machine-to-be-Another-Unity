using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServoExperimentManager : MonoBehaviour
{
    public Transform videoFlipParent;

    //public bool startFlashing; 
    public bool invertDirection, calibrate, screenOnOff, beginTask;
    [HideInInspector]
    public bool flipImage;

    public ArduinoControl _arduinoControl;
    public string port; 

    private bool wasFlashing, wasInverted, wasFlipped;

    public static ServoExperimentManager instance;

    private void Awake()
    {
        if (instance == null)     instance = this;
        _arduinoControl.manualPort = port;
    }

    private void Start()
    {
        screenOnOff = true;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(startFlashing != wasFlashing) {
            Blinker.instance.SetBlink(startFlashing);
            wasFlashing = startFlashing;
        }*/

        if (beginTask) HeadRotationTask.instance.beginTask = true;
        else HeadRotationTask.instance.beginTask = false;

        if (invertDirection != wasInverted)
        {
            ArduinoControl.instance.inverse = invertDirection;
            wasInverted = invertDirection;
        }

        if(flipImage != wasFlipped){
            if (flipImage)
                videoFlipParent.localScale = new Vector3(videoFlipParent.localScale.x, -1, videoFlipParent.localScale.z);
            else
                videoFlipParent.localScale = new Vector3(videoFlipParent.localScale.x, 1, videoFlipParent.localScale.z);
            wasFlipped = flipImage;
        }

        if(calibrate){
            VideoFeed.instance.RecenterPose();
            calibrate = false;
        }

        if (screenOnOff)
        {
            VideoFeed.instance.SetDimmed();
            screenOnOff = false;
        }

    }
}
