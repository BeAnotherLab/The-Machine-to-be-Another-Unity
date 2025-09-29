using System;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineControlller : MonoBehaviour //TODO rename. this is is for manual set up only (because experience running)
{
    private TrackAsset _polishTrack;
    private TrackAsset _englishTrack;

    [SerializeField] private PlayableDirector _instructionsTimeline;

    public delegate void OnHideImages();
    public static OnHideImages HideImages;
    
    private void OnEnable()
    {
        UserStateManager.BothUsersReady += StartSequencer;
        ArduinoManager.SerialFailure += StopSequencer;
        OscManager.ReceiveSerialFailure += StopSequencer;
        
        UserStateManager.OtherLeft += StopSequencer;
        UserStateManager.ThisUserLeft += StopSequencer;
        
    }

    private void OnDisable()
    {
        UserStateManager.BothUsersReady -= StartSequencer;
        ArduinoManager.SerialFailure -= StopSequencer;
        OscManager.ReceiveSerialFailure -= StopSequencer;
        UserStateManager.OtherLeft -= StopSequencer;
        UserStateManager.ThisUserLeft -= StopSequencer;
    }

    private void Awake()
    {
        TimelineAsset timelineAsset = (TimelineAsset) _instructionsTimeline.playableAsset;
        _englishTrack = timelineAsset.GetOutputTrack(0);
        _polishTrack = timelineAsset.GetOutputTrack(1);
    }

    public void SwitchLanguageTrack(string language)
    {
        _englishTrack.muted = language != "English";
        _polishTrack.muted = language != "Polish";
        _instructionsTimeline.RebuildGraph();
        _instructionsTimeline.Evaluate();
    }

    private void StartSequencer()
    {
        _instructionsTimeline.Play();
    }

    private void StopSequencer()
    {
        _instructionsTimeline.Stop();
        _instructionsTimeline.time = 0;
        _instructionsTimeline.Evaluate();
        
        HideImages();
    }

}
