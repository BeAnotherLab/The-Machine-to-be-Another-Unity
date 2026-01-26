using System;
using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour //TODO remove?
{
    public delegate void OnPlayingInstruction();
    public static OnPlayingInstruction PlayingInstruction = delegate { };
    
    public delegate void OnInstructionFinished();
    public static OnInstructionFinished FinishedInstruction = delegate { };
    
    [SerializeField] private AudioSource _audioSource; //the audio source we will play audio through
    
    [SerializeField] private bool _isAnyPlaying;
    
    private bool _wasAnyInstructionPlaying;
    
    private float _checkInterval = 0.1f;
    private float _checkTimer;
    
    private void OnEnable()
    {
        DataLoader.PlayInstruction += PlaySound;
        
        //TODO on sequence stop, stop audio
        //TODO ie use the user event that triggered that or a sequence event?
    }

    private void OnDisable()
    {
        DataLoader.PlayInstruction -= PlaySound;
    }

    private void Update()
    {
       MonitorInstructionAudio();
    }

    public void PlaySound(AudioClip clip)
    {
        if (!_isAnyPlaying)
        {
            _audioSource.clip = clip; 
            _audioSource.Play();
        }
    }
    
    private void MonitorInstructionAudio()
    {
        _checkTimer += Time.deltaTime;
        if (_checkTimer < _checkInterval) return;
        _checkTimer = 0f;

        _isAnyPlaying = _audioSource.isPlaying;
        
        if (!_wasAnyInstructionPlaying && _isAnyPlaying) PlayingInstruction();
        else if (_wasAnyInstructionPlaying && !_isAnyPlaying) FinishedInstruction();
        
        _wasAnyInstructionPlaying = _isAnyPlaying;
    }
    
    private IEnumerator WaitForAudioEnd()
    {
        PlayingInstruction();
        yield return new WaitWhile(() => _audioSource.isPlaying);
        FinishedInstruction();
    }
}
