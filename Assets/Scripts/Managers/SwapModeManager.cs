﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapModeManager : MonoBehaviour
{

    public static SwapModeManager instance;

    public enum SwapModes {AUTO_SWAP, MANUAL_SWAP, CURTAIN_MANUAL_SWAP, SERVO_SWAP};

    public SwapModes swapMode;

    public bool ArduinoControl; //for auto swap, enable if we are using an Arduino controlled system.

    public delegate void OnSwapModeChanged(SwapModes swapmodes);
    public static OnSwapModeChanged SwapModeChanged = delegate(SwapModes modes) {  };

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetSwapMode(swapMode);
        SwapModeChanged(swapMode);
    }

    public void SetSwapMode(SwapModes mode)
    {
        
        switch (mode)
        {
            
            case SwapModes.AUTO_SWAP:
                
                SettingsGUI.instance.SetSwapMode(ArduinoControl); //hide serial port dropdown, show repeater toggle, show IP input field
                VideoFeed.instance.twoWayWap = true; //move video with other pose
                StatusManager.instance.Standby(true, true); //go to initial state
                OscManager.instance.EnableRepeater(true); //enable OSC repeat
                OscManager.instance.SetSendHeadtracking(true); //send headtracking

                //enable serial depending on if we are using the curtain or not
                if (ArduinoControl) ArduinoManager.instance.ActivateSerial(false, ArduinoControl); //TODO remove?
                else ArduinoManager.instance.DisableSerial();
                
                break;

            case SwapModes.MANUAL_SWAP:
                
                SettingsGUI.instance.SetSwapMode(); //hide serial port dropdown, show repeater toggle, show IP input field
                VideoFeed.instance.twoWayWap = true; //move video with other pose
                StatusManager.instance.Standby(true, false); //go to initial state
                OscManager.instance.EnableRepeater(true); //enable OSC repeat
                OscManager.instance.SetSendHeadtracking(true); //send headtracking
                ArduinoManager.instance.DisableSerial(); //deactivate servos
                AudioManager.instance.StopAudioInstructions(); //stop auto swap instructions audio

                break;
                
            case SwapModes.CURTAIN_MANUAL_SWAP:
                
                SettingsGUI.instance.SetSwapMode(ArduinoControl); //hide serial port dropdown, show repeater toggle, show IP input field
                VideoFeed.instance.twoWayWap = true; //move video with other pose
                StatusManager.instance.Standby(true, false); //go to initial state
                OscManager.instance.EnableRepeater(true); //enable OSC repeat
                OscManager.instance.SetSendHeadtracking(true); //send headtracking
                if (ArduinoControl) ArduinoManager.instance.ActivateSerial(false, ArduinoControl); //TODO remove?
                //ArduinoManager.instance.DisableSerial(); //deactivate servos
                AudioManager.instance.StopAudioInstructions(); //stop auto swap instructions audio

                break;

            case SwapModes.SERVO_SWAP:
                
                ArduinoManager.instance.ActivateSerial(true, false); //enable servos
                SettingsGUI.instance.SetServoMode(); //show serial port dropdown, hide repeater toggle, hide IP input field
                VideoFeed.instance.twoWayWap = false; //keep video in front of camera
                AudioManager.instance.StopAudioInstructions(); //stop auto swap instructions audio
                break;
        }

        swapMode = mode;
        PlayerPrefs.SetInt("swapMode", (int) mode);

    }
    
}
