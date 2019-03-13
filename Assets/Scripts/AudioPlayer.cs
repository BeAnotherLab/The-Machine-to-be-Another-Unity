﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class AudioPlayer : MonoBehaviour {

    #region Public Fields

    public static AudioPlayer instance;

    public int language;

    #endregion

    #region Private Fields


    [SerializeField]
    private AudioSource[] _englishClips; //english audios
    [SerializeField]
    private AudioSource[] _frenchClips; //french audios
    [SerializeField]
    private AudioSource[] _portugueseClips; //portuguese audios

    private List<AudioSource[]> _audioClips;

    [SerializeField]
    private AudioSource _music; //the background music
    [SerializeField]
    private AudioSource _autoModeInstructions; //the audio file played when in automatic mode
    
    private bool _somethingIsPlaying;

    #endregion

    #region MonoBehaviour Methods


    private void Awake()
    {
        if (instance == null) instance = this;

        _audioClips = new List<AudioSource[]>();

        _englishClips = GameObject.Find("EnglishAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_englishClips);

        _frenchClips = GameObject.Find("FrenchAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_frenchClips);

        _portugueseClips = GameObject.Find("PortugueseAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_portugueseClips);        
    }

    // Use this for initialization
    private void Start()
    {
        //play looping background music
        _music.loop = true;
        _music.Play();

        language = PlayerPrefs.GetInt("language");

        foreach (AudioSource clip in _audioClips[language])
            clip.Pause();
    }

    // Update is called once per frame
       private void Update()
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(vKey))
                {
                    if (vKey == UnityEngine.KeyCode.F)
                        playSound(11);
                    else if (vKey == UnityEngine.KeyCode.E)
                        playSound(0);
                    else if (vKey == UnityEngine.KeyCode.R)
                        playSound(1);
                    else if (vKey == UnityEngine.KeyCode.T)
                        playSound(2);
                    else if (vKey == UnityEngine.KeyCode.Y)
                        playSound(3);
                    else if (vKey == UnityEngine.KeyCode.U)
                        playSound(4);
                    else if (vKey == UnityEngine.KeyCode.I)
                        playSound(5);
                    else if (vKey == UnityEngine.KeyCode.O)
                        playSound(6);
                    else if (vKey == UnityEngine.KeyCode.P)
                        playSound(7);
                    else if (vKey == UnityEngine.KeyCode.Q)
                        playSound(8);
                    else if (vKey == UnityEngine.KeyCode.S)
                        playSound(9);
                    else if (vKey == UnityEngine.KeyCode.D)
                        playSound(10);
                }
            }

            _somethingIsPlaying = false;

        //check if some audio is playing 
        for (int i = 0; i < _audioClips[language].Length; i++)
        {
            if (_audioClips[language][i].isPlaying)
                _somethingIsPlaying = true;
        }

        if (!_somethingIsPlaying)
            _music.volume = 1;

    }

    #endregion


    #region Public Methods
    
    public void PlayAudioInstructions()
    {
        _autoModeInstructions.Play();
    }

    public void StopAudioInstructions()
    {
        _autoModeInstructions.Stop();
    }

    public void playSound(int id)
    {
        if (!_somethingIsPlaying)
        {
            Debug.Log("playing sound" + id.ToString());
            _audioClips[language][id].Play();
            _music.volume = 0.45f;
        }
    }
    #endregion


    #region Private Methods
    #endregion

}
