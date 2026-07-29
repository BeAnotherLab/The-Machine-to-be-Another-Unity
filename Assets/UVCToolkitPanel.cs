using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Serenegiant.UVC
{
    public class UVCToolkitPanel : MonoBehaviour
    {
        [SerializeField] private UVCManager _manager;
        [SerializeField] private UIDocument _document;
        
        private Button _showConsoleButton;
        private DropdownField _cameraDropdown;
        private DropdownField _resolutionDropdown;
        private ScrollView _controlsContainer;

        private List<UVCManager.CameraInfo> _cameras = new();
        private UVCManager.CameraInfo _currentCamera;
        private List<UVCManager.CameraInfo> _lastCameras = new();
        
        private const string PREF_WIDTH = "uvc_width";
        private const string PREF_HEIGHT = "uvc_height";
        
        private const ulong AUTO_EXPOSURE = 0x2;
        private const ulong AUTO_EXPOSURE_PRIORITY = 0x4;
        private const ulong EXPOSURE = 0x8;
        
        private void Start()
        {
            var root = _document.rootVisualElement;

            _showConsoleButton = root.Q<Button>("show-console-button");
            
            //showConsoleButton.clicked += () => LunarConsole.Show();
            _cameraDropdown = root.Q<DropdownField>("cameraDropdown");
            _resolutionDropdown = root.Q<DropdownField>("resolutionDropdown");
            _controlsContainer = root.Q<ScrollView>("controlsContainer");
            _cameraDropdown.RegisterValueChangedCallback(OnCameraChanged);
            _resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);

            Refresh();
        }

        private void Update()
        {
            var current = _manager.GetAttachedDevices();

            if (current.Count != _lastCameras.Count)
            {
                Refresh();
                _lastCameras = current;
            }
        }
        
        private void Refresh()
        {
            _cameras = _manager.GetAttachedDevices();
            Debug.Log($"Refresh: found {_cameras.Count} cameras");
            _cameraDropdown.choices.Clear();
            foreach (var camera in _cameras) _cameraDropdown.choices.Add(camera.DeviceName);
            if (_cameras.Count > 0)
            {
                _cameraDropdown.index = 0;
                SelectCamera(0);
            }
        }

        private void OnCameraChanged(ChangeEvent<string> evt)
        {
            SelectCamera(_cameraDropdown.index);
        }

        private void SelectCamera(int index)
        {
            if (index < 0 || index >= _cameras.Count) return;
            _currentCamera = _cameras[index];
            BuildResolutionList();
            _currentCamera.UpdateCtrls();
            BuildControls();

            if (_currentCamera != null)
            {
                _currentCamera.SetValue(AUTO_EXPOSURE_PRIORITY, 0); //set auto exposure priority before setting value. not sure if this is the right value
                _currentCamera.SetValue(AUTO_EXPOSURE, 1); //this value allows us to set the exposure manually on the fhd01m
            }
        }

        private void BuildResolutionList()
        {
            _resolutionDropdown.choices.Clear();

            int savedWidth = PlayerPrefs.GetInt(PREF_WIDTH, -1);
            int savedHeight = PlayerPrefs.GetInt(PREF_HEIGHT, -1);
            int selectedIndex = 0;
            for (int i = 0; i < _currentCamera.SupportedSizes.Length; i++)
            {
                var size = _currentCamera.SupportedSizes[i];
                _resolutionDropdown.choices.Add($"{size.Width}x{size.Height}");
                if (size.Width == savedWidth && size.Height == savedHeight) selectedIndex = i;
            }

            _resolutionDropdown.index = selectedIndex;
        }
        private void OnResolutionChanged(ChangeEvent<string> evt)
        {
            if (_currentCamera == null) return;
            int index = _resolutionDropdown.index;
            if (index < 0) return;
            var size = _currentCamera.SupportedSizes[index];
            PlayerPrefs.SetInt(PREF_WIDTH, (int)size.Width);
            PlayerPrefs.SetInt(PREF_HEIGHT, (int)size.Height);
            PlayerPrefs.Save();
            Debug.Log($"Saved default resolution: {size.Width}x{size.Height}");
        }

        private void BuildControls()
        {
            _controlsContainer.Clear();

            if (_currentCamera == null)
            {
                Debug.Log("No camera selected");
                return;
            }

            CreateExposure();
        }

        private void CreateExposure()
        {
            try
            {
                var info = _currentCamera.GetInfo(EXPOSURE);
                int current = _currentCamera.GetValue(EXPOSURE);
                var slider = new SliderInt((int)info.min, (int)info.max) { value = current };
                slider.label = "Exposure";
                slider.RegisterValueChangedCallback(evt =>
                {
                    _currentCamera.SetValue(EXPOSURE, evt.newValue);
                    Debug.Log($"Exposure = {evt.newValue}");
                });
                _controlsContainer.Add(slider);
            }
            catch (Exception e)
            {
                Debug.Log($"Exposure not available: {e.Message}");
            }
        }
    }
}