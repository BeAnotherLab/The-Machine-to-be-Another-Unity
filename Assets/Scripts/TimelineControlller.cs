using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelineControlller : MonoBehaviour
{
    [SerializeField] private PlayableDirector _instructionsTimeline;

    private TrackAsset _germanTrack;
    private TrackAsset _englishTrack;

    private void OnEnable()
    {
        UserStateManager.BothUsersReady += StartSequencer;
        StatusManager.StopSequencer += StopSequencer;
    }

    private void OnDisable()
    {
        UserStateManager.BothUsersReady += StartSequencer;
        StatusManager.StopSequencer -= StopSequencer;
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
}
