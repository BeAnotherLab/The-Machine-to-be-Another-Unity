using System.Collections;
using System.Collections.Generic;
using RenderHeads.Media.AVProLiveCamera;
using UnityEngine;

public class CognitiveTestCameraManager : AbstractAVProLiveCameraSwitcher
{

    public static CognitiveTestCameraManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
}
