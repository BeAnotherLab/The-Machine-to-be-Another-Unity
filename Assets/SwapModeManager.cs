﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapModeManager : MonoBehaviour
{

    public static SwapModeManager instance;

    public enum SwapModes {AUTO_SWAP, MANUAL_SWAP, SERVO_SWAP};

    public SwapModes swapMode;

    public bool useCurtain;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //load swap mode from player prefs
        if (PlayerPrefs.GetInt("swapMode", 0) == 0 ) SetSwapMode(SwapModes.AUTO_SWAP);
        else if (PlayerPrefs.GetInt("swapMode", 0) == 1 ) SetSwapMode(SwapModes.MANUAL_SWAP);
        else if (PlayerPrefs.GetInt("swapMode", 0) == 2 ) SetSwapMode(SwapModes.SERVO_SWAP);
    }

    public void SetSwapMode(SwapModes mode)
    {
        switch (mode)
        {
            case SwapModes.AUTO_SWAP:
                //deactivate servos
                if(!useCurtain)
                    ArduinoControl.instance.ActivateServos(false);
                else
                    ArduinoControl.instance.ActivateServos(true);

                //hide serial port dropdown, show repeater toggle, show IP input field
                SettingsGUI.instance.SetSwapMode();

                //move video with other pose
                VideoFeed.instance.twoWayWap = true;

                //enable status management, self, other, autoplay, autofinish, reset timer      
                StatusManager.instance.StopExperience();
                StatusManager.instance.SetAutoStartAndFinish(true, 22);
                StatusManager.instance.statusManagementOn = true;

                //enable OSC repeat
                OscManager.instance.EnableRepeater(true);

                break;

            case SwapModes.MANUAL_SWAP:
                //deactivate servos
                ArduinoControl.instance.ActivateServos(false);

                //hide serial port dropdown, show repeater toggle, show IP input field
                SettingsGUI.instance.SetSwapMode();

                //move video with other pose
                VideoFeed.instance.twoWayWap = true;

                //enable status management, self, other, remove autoplay, autofinish, reset timer               
                StatusManager.instance.StopExperience();
                StatusManager.instance.SetAutoStartAndFinish(false, 5);
                StatusManager.instance.statusManagementOn = true;

                //stop auto swap instructions audio
                AudioPlayer.instance.StopAudioInstructions();

                //enable OSC repeat
                OscManager.instance.EnableRepeater(true);

                break;

            case SwapModes.SERVO_SWAP:
                //enable servos
                ArduinoControl.instance.ActivateServos(true);

                //show serial port dropdown, hide repeater toggle, hide IP input field
                SettingsGUI.instance.SetServoMode();

                //keep video in front of camera
                VideoFeed.instance.twoWayWap = false;

                //disable status management
                StatusManager.instance.DisableStatusManagement();

                //stop auto swap instructions audio
                AudioPlayer.instance.StopAudioInstructions();

                //disable OSC repeat
                OscManager.instance.EnableRepeater(false);

                break;
        }

        swapMode = mode;
        PlayerPrefs.SetInt("swapMode", (int) mode);

    }
}