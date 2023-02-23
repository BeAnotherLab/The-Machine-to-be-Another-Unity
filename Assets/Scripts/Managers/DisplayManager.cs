﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Pong;
using Uduino;
using UnityEngine;

public enum DisplayMode{Debug, Prod}

public class DisplayManager : MonoBehaviour //This manager centralizes display of screens and menus on multidisplay setups
{
    public DisplayMode displayMode;
    public static DisplayManager instance;

    public delegate void OnSetDisplayMode(bool show);
    public static OnSetDisplayMode SetDisplayModeEvent = delegate(bool show) {  };

    [SerializeField] private GameObject _utilities;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if(Display.displays.Length > 1) Display.displays[1].Activate();
        SetDisplayMode(displayMode);
    }

    public void SetDisplayMode(DisplayMode displayMode)
    {
        var show = displayMode == DisplayMode.Debug;
        
        //hide menus
        _utilities.SetActive(show);
        VideoCameraManager.instance.EnableDeviceMenu(show);
        SetDisplayModeEvent(show);
        
        this.displayMode = displayMode;
    }

    public void ToggleDisplayMode()
    {
        if (displayMode == DisplayMode.Debug)
            SetDisplayMode(DisplayMode.Prod);
        else 
            SetDisplayMode(DisplayMode.Debug);
    }
}