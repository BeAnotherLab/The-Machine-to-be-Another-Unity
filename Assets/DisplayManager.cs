using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Pong;
using Uduino;
using UnityEngine;

public enum DisplayMode{Debug, Prod}

public class DisplayManager : MonoBehaviour //This manager centralizes display of screens and menus on multidisplay setups
{

    public DisplayMode displayMode;

    // Start is called before the first frame update
    void Start()
    {
        if(Display.displays.Length > 1) Display.displays[1].Activate();

        if (displayMode == DisplayMode.Prod)
        {
            //hide menus
            GameObject.Find("Utilities").SetActive(false);
            VideoCameraManager.instance.EnableDeviceMenu(false);
            SettingsGUI.instance.SetMonitorGuiEnabled(false);
            CustomNetworkManager.instance.EnableNetworkGUI(false);
        }
    }
    
}
