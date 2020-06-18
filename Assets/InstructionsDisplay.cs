using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class InstructionsDisplay : MonoBehaviour
{
    [SerializeField] private VideoPlayer _welcomeLoopVideo;
    [SerializeField] private VideoPlayer _waitForTurnLoopVideo;
    [SerializeField] private GameObject _technicalFailurePanel;

    public static InstructionsDisplay instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if(Display.displays.Length > 1)
            Display.displays[1].Activate();
    }

    public void ShowWelcomeVideo()
    {
        _waitForTurnLoopVideo.Stop();
        _welcomeLoopVideo.Play();
    }

    public void ShowWaitForTurnVideo()
    {
        _welcomeLoopVideo.Stop();
        _waitForTurnLoopVideo.Play();
    }
    
    public void ShowTechnicalFailureMessage()
    {
        _technicalFailurePanel.gameObject.SetActive(true);
    }
}
