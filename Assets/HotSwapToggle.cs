using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using UnityEngine;

public class HotSwapToggle : MonoBehaviour
{
    public void SetHotSwap(bool hotSwap)
    {
        GetComponent<AVProLiveCameraManager>()._supportHotSwapping = hotSwap;
        GetComponent<AVProLiveCamera>()._updateHotSwap = hotSwap;
        PlayerPrefs.SetInt("hotSwap", hotSwap? 1:0);
    }
}
