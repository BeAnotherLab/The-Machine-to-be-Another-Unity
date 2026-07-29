using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Serenegiant.UVC
{
    public class UVCToolkitPanel : MonoBehaviour
    {
        [SerializeField] private UVCManager manager;
        [SerializeField] private UIDocument document;
        
        private Button showConsoleButton;
        private DropdownField cameraDropdown;
        private DropdownField resolutionDropdown;
        private ScrollView controlsContainer;

        private List<UVCManager.CameraInfo> cameras = new();

        private UVCManager.CameraInfo currentCamera;
        
        private List<UVCManager.CameraInfo> lastCameras = new();
        
        private const string PREF_WIDTH = "uvc_width";
        private const string PREF_HEIGHT = "uvc_height";
        
        private const ulong AUTO_EXPOSURE = 0x2;
        private const ulong AUTO_EXPOSURE_PRIORITY = 0x4;
        private const ulong EXPOSURE = 0x8;
        
        private void Start()
        {
            var root = document.rootVisualElement;

            showConsoleButton = root.Q<Button>("show-console-button");
            
            //showConsoleButton.clicked += () => LunarConsole.Show();
            cameraDropdown = root.Q<DropdownField>("cameraDropdown");
            resolutionDropdown = root.Q<DropdownField>("resolutionDropdown");
            controlsContainer = root.Q<ScrollView>("controlsContainer");
            cameraDropdown.RegisterValueChangedCallback(OnCameraChanged);
            resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);

            Refresh();
        }

        private void Update()
        {
            var current = manager.GetAttachedDevices();

            if (current.Count != lastCameras.Count)
            {
                Refresh();
                lastCameras = current;
            }
        }
        
        public void Refresh()
        {
            cameras = manager.GetAttachedDevices();
            Debug.Log($"Refresh: found {cameras.Count} cameras");
            cameraDropdown.choices.Clear();
            foreach (var camera in cameras) cameraDropdown.choices.Add(camera.DeviceName);
            if (cameras.Count > 0)
            {
                cameraDropdown.index = 0;
                SelectCamera(0);
            }
        }

        void OnCameraChanged(ChangeEvent<string> evt)
        {
            SelectCamera(cameraDropdown.index);
        }

        void SelectCamera(int index)
        {
            if (index < 0 || index >= cameras.Count) return;
            currentCamera = cameras[index];
            BuildResolutionList();
            currentCamera.UpdateCtrls();
            BuildControls();

            if (currentCamera != null)
            {
                currentCamera.SetValue(AUTO_EXPOSURE_PRIORITY, 0); //set auto exposure priority before setting value. not sure if this is the right value
                currentCamera.SetValue(AUTO_EXPOSURE, 1); //this value allows us to set the exposure manually on the fhd01m
            }
        }

        void BuildResolutionList()
        {
            resolutionDropdown.choices.Clear();

            int savedWidth = PlayerPrefs.GetInt(PREF_WIDTH, -1);
            int savedHeight = PlayerPrefs.GetInt(PREF_HEIGHT, -1);
            int selectedIndex = 0;
            for (int i = 0; i < currentCamera.SupportedSizes.Length; i++)
            {
                var size = currentCamera.SupportedSizes[i];
                resolutionDropdown.choices.Add($"{size.Width}x{size.Height}");
                if (size.Width == savedWidth && size.Height == savedHeight) selectedIndex = i;
            }

            resolutionDropdown.index = selectedIndex;
        }
        void OnResolutionChanged(ChangeEvent<string> evt)
        {
            if (currentCamera == null) return;
            int index = resolutionDropdown.index;
            if (index < 0) return;
            var size = currentCamera.SupportedSizes[index];
            PlayerPrefs.SetInt(PREF_WIDTH, (int)size.Width);
            PlayerPrefs.SetInt(PREF_HEIGHT, (int)size.Height);
            PlayerPrefs.Save();
            Debug.Log($"Saved default resolution: {size.Width}x{size.Height}");
        }

        void BuildControls()
        {
            controlsContainer.Clear();

            if (currentCamera == null)
            {
                Debug.Log("No camera selected");
                return;
            }

            CreateExposure();
        }

        void CreateExposure()
        {
            try
            {
                var info = currentCamera.GetInfo(EXPOSURE);
                int current = currentCamera.GetValue(EXPOSURE);
                var slider = new SliderInt((int)info.min, (int)info.max) { value = current };
                slider.label = "Exposure";
                slider.RegisterValueChangedCallback(evt =>
                {
                    currentCamera.SetValue(EXPOSURE, evt.newValue);
                    Debug.Log($"Exposure = {evt.newValue}");
                });
                controlsContainer.Add(slider);
            }
            catch (Exception e)
            {
                Debug.Log($"Exposure not available: {e.Message}");
            }
        }
    }
}