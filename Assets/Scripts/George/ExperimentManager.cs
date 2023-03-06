using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using ScriptableObjectArchitecture;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class ExperimentManager : MonoBehaviour
{
    private Stopwatch _stopwatch;
    
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private FloatGameEvent _stopWatchTime;
    [SerializeField] private float _phaseLength;
    
    [SerializeField] private AudioSource _leaderAudioSource;
    [SerializeField] private AudioSource _followerAudioSource;
    [SerializeField] private AudioSource _freeAudioSource;
    
    private bool _phaseRunning;

    private void Awake()
    {
        _stopwatch = new Stopwatch();
    }

    public void StartExperiment()
    {
        _stopwatch.Start();
        StartCoroutine(WaitForClipEnd());
        _phaseRunning = true;
        VideoFeed.instance.Dim(false);
    }
    
    public void Setrole(int role)
    {
        if (role == 0)
        {
            _leaderAudioSource.Play();
        }
        if (role == 1)
        {
            _followerAudioSource.Play();
        }
        if (role == 2)
        {
            _freeAudioSource.Play();
        }
    }
    
    private IEnumerator WaitForClipEnd()
    {
        yield return new WaitForSeconds(_phaseLength);
        Debug.Log("phase finished");
        _phaseRunning = false;
        _stopwatch.Stop();
        _stopwatch.Reset();
        _stopWatchTime.Raise(0);
        VideoFeed.instance.Dim(true);
    }

    private void Update()
    {
        if(_phaseRunning)
        {
            _stopWatchTime.Raise(_phaseLength- _stopwatch.ElapsedMilliseconds/1000);
        }
    }
}
