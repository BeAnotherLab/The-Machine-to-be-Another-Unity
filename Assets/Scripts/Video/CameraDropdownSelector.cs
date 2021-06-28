using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public abstract class CameraDropdownSelector : MonoBehaviour
{
    public void SetCameraDropdownOptions(Dropdown dropdown)
    {
        dropdown.options.Clear();

        for(int i = 0; i < AVProLiveCameraManager.Instance.NumDevices; i++)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = AVProLiveCameraManager.Instance.GetDevice(i).Name });
        }
        dropdown.value = PlayerPrefs.GetInt("cameraID");
        dropdown.RefreshShownValue();
    }
}
