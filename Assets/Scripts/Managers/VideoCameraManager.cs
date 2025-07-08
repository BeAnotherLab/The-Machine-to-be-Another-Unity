using System;
using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using RenderHeads.Media.AVProLiveCamera.Demos;
using UnityEngine;
using UnityEngine.UI;

public class VideoCameraManager : AbstractAVProLiveCameraSwitcher
{
    public static VideoCameraManager instance;

    private AVProLiveCamera _avProLiveCamera;

    private void OnEnable()
    {
        SettingsGUI.ExposureValueChanged += UpdateExposure;
    }

    private void OnDisable()
    {
        SettingsGUI.ExposureValueChanged -= UpdateExposure;
    }

    private void Awake()
    {
        if (instance == null) instance = this;
        _avProLiveCamera = FindObjectOfType<AVProLiveCamera>();
    }

    private void Start()
    {
        _avProLiveCamera._deviceSelection = AVProLiveCamera.SelectDeviceBy.Name;
        _avProLiveCamera._desiredModeIndex = PlayerPrefs.GetInt("CameraModeIndex");
        _avProLiveCamera._desiredDeviceNames.Add(PlayerPrefs.GetString("CameraName"));
        _avProLiveCamera._desiredFrameRate = Single.MaxValue;
        _avProLiveCamera.Begin();
    }

    public void ShowCameraConfigWindow()
    {
        _avProLiveCamera.Device.ShowConfigWindow();
    }
    
    public void EnableDeviceMenu(bool show)
    {
        GetComponent<CustomQuickDeviceMenu>().enabled = show;
    }
    
    private void UpdateExposure(int value)
    {
        AVProLiveCameraDevice device = _avProLiveCamera.Device;
        if( device != null)
        {
            AVProLiveCameraSettingBase settingBase = device.GetVideoSettingByType(AVProLiveCameraDevice.SettingsEnum.Exposure);
            
            if (settingBase != null && settingBase is AVProLiveCameraSettingFloat)
            {
                AVProLiveCameraSettingFloat setting = settingBase as AVProLiveCameraSettingFloat;
                setting.IsAutomatic = false;
                setting.CurrentValue = value;
                setting.Update();
            }
        }
    }
}
