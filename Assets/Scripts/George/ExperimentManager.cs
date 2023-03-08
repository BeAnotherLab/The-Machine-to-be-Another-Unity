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
    
    private bool _phaseRunning;
    private string _language;

    [SerializeField] private AudioSource _germanLeaderAudioSource;
    [SerializeField] private AudioSource _germanFollowerAudioSource;
    [SerializeField] private AudioSource _germanFreeAudioSource;
    [SerializeField] private AudioSource _germanPreparationAudioSource;
    
    [SerializeField] private AudioSource _englishLeaderAudioSource;
    [SerializeField] private AudioSource _englishFollowerAudioSource;
    [SerializeField] private AudioSource _englishFreeAudioSource;
    [SerializeField] private AudioSource _englishPreparationAudioSource;

    private void Awake()
    {
        _stopwatch = new Stopwatch();
    }

    private void Start()
    {
        ChangeLanguage("German");
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
            if (_language == "German") _germanLeaderAudioSource.Play();
            else if (_language == "English") _englishLeaderAudioSource.Play();
        }
        if (role == 1)
        {
            if (_language == "German") _germanFollowerAudioSource.Play();
            else if (_language == "English") _englishFollowerAudioSource.Play();
        }
        if (role == 2)
        {
            if (_language == "German") _germanFreeAudioSource.Play();
            else if (_language == "English") _englishFreeAudioSource.Play();
        }
    }
    
    public void PlayPreparation()
    {
        if (_language == "German") _germanPreparationAudioSource.Play();
        else if (_language == "English") _englishPreparationAudioSource.Play();
    }
    
    public void ChangeLanguage(string language)
    {
        _language = language;
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
