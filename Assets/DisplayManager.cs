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
    }

    public void SetDisplayMode(DisplayMode displayMode)
    {
        var show = this.displayMode == DisplayMode.Debug;
        
        //hide menus
        GameObject.Find("Utilities").SetActive(show);
        VideoCameraManager.instance.EnableDeviceMenu(show);
        SettingsGUI.instance.SetMonitorGuiEnabled(show);
        CustomNetworkManager.instance.EnableNetworkGUI(show);
    }
    
}
