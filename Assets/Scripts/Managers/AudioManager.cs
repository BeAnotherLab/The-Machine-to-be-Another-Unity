using System;
using UnityEngine;

public class AudioManager : MonoBehaviour //Only for Manual modes. Not present in Auto Body Swap 
{
    public delegate void OnPlayingInstruction();
    public static OnPlayingInstruction PlayingInstruction;
    
    public delegate void OnInstructionFinished();
    public static OnInstructionFinished FinishedInstruction;

    
    [SerializeField] private AudioSource[] _audioClips;
    [SerializeField] private AudioSource[] _autoModeInstructions; //the audio file played when in automatic mode
    
    private bool _wasAnyInstructionPlaying;
    
    private float _checkInterval = 0.1f;
    private float _checkTimer;
    
    private void OnEnable()
    {
        OscManager.ReceivedAudioButtonPressed += PlaySound;
    }

    private void OnDisable()
    {
        OscManager.ReceivedAudioButtonPressed -= PlaySound;
    }

    private void Awake()
    {
        _audioClips = GameObject.Find("AudioInstructions").GetComponentsInChildren<AudioSource>();
    }

    private void Update()
    {
       HandlePlayInput();
       MonitorInstructionAudio();
    }

    public void PlaySound(int id)
    {
        if (id < 0 || id >= _audioClips.Length) return;

        if (!_audioClips[id].isPlaying) _audioClips[id].Play();
    }
    
    private void MonitorInstructionAudio()
    {
        _checkTimer += Time.deltaTime;
        if (_checkTimer < _checkInterval) return;
        _checkTimer = 0f;

        var isAnyPlaying = false;

        foreach (var clip in _audioClips)
        {
            if (clip != null && clip.isPlaying)
            {
                isAnyPlaying = true;
                break;
            }
        }

        if (!_wasAnyInstructionPlaying && isAnyPlaying) PlayingInstruction();
        else if (_wasAnyInstructionPlaying && !isAnyPlaying) FinishedInstruction();
        

        _wasAnyInstructionPlaying = isAnyPlaying;
    }
    
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
    
}
