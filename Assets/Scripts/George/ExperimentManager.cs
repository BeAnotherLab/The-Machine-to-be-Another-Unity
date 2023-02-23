using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ExperimentManager : MonoBehaviour
{
    // Start is called before the first frame update
    
    //receive start experiment messages
    //start timer
    //stop timer
    //Update feedback UI
    
    private Stopwatch _stopwatch;
    
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private FloatGameEvent _stopWatchTime;
    [SerializeField] private float _phaseLength;
    
    [SerializeField] private AudioSource _leaderAudioSource;
    [SerializeField] private AudioSource _followerAudioSource;
    [SerializeField] private AudioSource _freeAudioSource;
    
    private bool _phaseRunning;
    
    public void StartExperiment()
    {
        _stopwatch.Start();
        StartCoroutine(WaitForClipEnd());
        _phaseRunning = true;
        
        if (_experimentData.participantType == ParticipantType.leader)
        {
            _leaderAudioSource.Play();
        }
        if (_experimentData.participantType == ParticipantType.follower)
        {
            _followerAudioSource.Play();
        }
        else if (_experimentData.participantType == ParticipantType.free)
        {
            _freeAudioSource.Play();
        }
    }
    
    private IEnumerator WaitForClipEnd()
    {
        yield return new WaitForSeconds(_phaseLength);
        Debug.Log("phase finished");
        _phaseRunning = false;
    }

    private void Update()
    {
        if(_phaseRunning)
        {
            _stopWatchTime.Raise(_stopwatch.ElapsedMilliseconds);
        }
    }
}
