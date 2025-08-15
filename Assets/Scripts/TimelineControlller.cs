using System;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineControlller : MonoBehaviour
{
        private TrackAsset _germanTrack;
    private TrackAsset _englishTrack;

    [SerializeField] private BoolVariable _experienceRunning;
    [SerializeField] private PlayableDirector _instructionsTimeline;

    private void OnEnable()
    {
        UserStateManager.BothUsersReady += StartSequencer;

        ArduinoManager.SerialFailure += StopSequencer;
        OscManager.ReceiveSerialFailure += StopSequencer;
        UserStateManager.StopSequencer += StopSequencer;
        
        _instructionsTimeline.played += Playing;
        _instructionsTimeline.paused += Paused;
    }

    private void OnDisable()
    {
        UserStateManager.BothUsersReady += StartSequencer;

        ArduinoManager.SerialFailure -= StopSequencer;
        OscManager.ReceiveSerialFailure -= StopSequencer;
        UserStateManager.StopSequencer -= StopSequencer;
        
        _instructionsTimeline.played -= Playing;
        _instructionsTimeline.paused -= Paused;
    }

    private void Awake()
    {
        TimelineAsset timelineAsset = (TimelineAsset) _instructionsTimeline.playableAsset;
        _englishTrack = timelineAsset.GetOutputTrack(0);
        _germanTrack = timelineAsset.GetOutputTrack(1);
    }

    public void SwitchLanguageTrack(string language)
    {
        _englishTrack.muted = language != "English";
        _germanTrack.muted = language != "German";
    }

    private void StartSequencer()
    {
        _instructionsTimeline.Play();
    }

    private void StopSequencer()
    {
        _instructionsTimeline.Stop();
    }

    private void Playing(PlayableDirector director)
    {
        _experienceRunning.Value = true;
    }
    
    private void Paused(PlayableDirector director)
    {
        _experienceRunning.Value = false;
    }
}
