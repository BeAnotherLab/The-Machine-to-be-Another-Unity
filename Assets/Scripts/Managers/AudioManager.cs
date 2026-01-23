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
       //HandlePlayInput();
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
    /*
    private void HandlePlayInput()
    {
        foreach (KeyCode vKey in Enum.GetValues(typeof(KeyCode))) //TODO remove keys?
        {
            if (Input.GetKey(vKey))
            {
                if (vKey == KeyCode.Q)
                    PlaySound(0);
                else if (vKey == KeyCode.W)
                    PlaySound(5);
                else if (vKey == KeyCode.E)
                    PlaySound(4);
                else if (vKey == KeyCode.R)
                    PlaySound(6);
                else if (vKey == KeyCode.T)
                    PlaySound(7);
                else if (vKey == KeyCode.Y)
                    PlaySound(1);
                else if (vKey == KeyCode.U)
                    PlaySound(2);
                else if (vKey == KeyCode.I)
                    PlaySound(3);
                else if (vKey == KeyCode.J)
                    PlaySound(8);
                else if (vKey == KeyCode.K)
                    PlaySound(9);
                else if (vKey == KeyCode.L)
                    PlaySound(10);
            }
        }
    }
    */
    
    
    private IEnumerator WaitForAudioEnd()
    {
        PlayingInstruction();
        yield return new WaitWhile(() => _audioSource.isPlaying);
        FinishedInstruction();
    }
}
