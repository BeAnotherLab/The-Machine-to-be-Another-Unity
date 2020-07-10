using System.Collections;
using System.Collections.Generic;
using Mirror.Examples.Pong;
using Uduino;
using UnityEngine;

public class DisplayManager : MonoBehaviour //This manager centralizes display of screens and menus on multidisplay setups
{
    public bool hideAllMenus;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("Utilities").SetActive(!hideAllMenus);
        VideoCameraManager.instance.EnableDeviceMenu(!hideAllMenus);
        SettingsGUI.instance.SetMonitorGuiEnabled(!hideAllMenus);
        CustomNetworkManager.instance.EnableNetworkGUI(!hideAllMenus);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
