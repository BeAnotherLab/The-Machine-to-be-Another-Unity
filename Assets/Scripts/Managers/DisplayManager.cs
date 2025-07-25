using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Pong;
using Uduino;
using UnityEngine;
using UnityEngine.Serialization;

public enum DisplayMode{Debug, Prod}

public class DisplayManager : MonoBehaviour //This manager centralizes display of screens and menus on multidisplay setups
{
    public DisplayMode displayMode;

    public delegate void OnSetDisplayMode(bool show);
    public static OnSetDisplayMode SetDisplayModeEvent = delegate(bool show) {  };

    [SerializeField] private GameObject _display; //TODO no direct reference

    private void OnEnable()
    {
        SettingsGUI.DebugMenuPressed += ToggleDisplayMode;
    }
    
    private void OnDisable()
    {
        SettingsGUI.DebugMenuPressed -= ToggleDisplayMode;
    }

    private void Start()
    {
        if(Display.displays.Length > 1) Display.displays[1].Activate();
        SetDisplayMode(displayMode);
    }

    public void SetDisplayMode(DisplayMode displayMode)
    {
        var show = displayMode == DisplayMode.Debug;
        
        //hide menus
        _display.SetActive(show);
        VideoCameraManager.instance.EnableDeviceMenu(show);
        SetDisplayModeEvent(show);
        
        this.displayMode = displayMode;
    }

    public void ToggleDisplayMode()
    {
        if (displayMode == DisplayMode.Debug) SetDisplayMode(DisplayMode.Prod);
        else SetDisplayMode(DisplayMode.Debug);
    }
}
