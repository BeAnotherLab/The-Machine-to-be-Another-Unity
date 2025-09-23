using System;
using UnityEngine;

public class AudioManager : MonoBehaviour //Only for Manual modes. Not present in Auto Body Swap 
{ 
    
    [SerializeField] private AudioSource[] _audioClips;

    [SerializeField] private AudioSource _music; //the background music
    [SerializeField] private AudioSource[] _autoModeInstructions; //the audio file played when in automatic mode
    
    private bool _somethingIsPlaying;
    private bool _lookForLanguageAudioClips;
    
    private void OnEnable()
    {
        StatusManager.StopAudiosInstructions += StopAudioInstructions;
        OscManager.ReceivedAudioButtonPressed += PlaySound;
    }

    private void OnDisable()
    {
        StatusManager.StopAudiosInstructions -= StopAudioInstructions;
        OscManager.ReceivedAudioButtonPressed -= PlaySound;
    }

    private void Awake()
    {
        _audioClips = GameObject.Find("AudioInstructions").GetComponentsInChildren<AudioSource>();
    }

    private void Start()
    {
        _music.loop = true; 
        _music.Play();

        foreach (AudioSource clip in _audioClips) clip.Pause();
    }

    // Update is called once per frame
    private void Update()
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

        _somethingIsPlaying = false;

        //check if some audio is playing 
        for (int i = 0; i < _audioClips.Length; i++) if (_audioClips[i].isPlaying) _somethingIsPlaying = true;

        if (!_somethingIsPlaying) _music.volume = 1;
    }

    private void StopAudioInstructions() //TODO remove?
    {
        foreach (AudioSource _instruction in _autoModeInstructions) _instruction.Stop();
    }

    private void PlaySound(int id)
    {
        if (!_somethingIsPlaying)
        {
            Debug.Log("playing sound" + id);
            _audioClips[id].Play();
            _music.volume = 0.45f;
        }
    }

}
