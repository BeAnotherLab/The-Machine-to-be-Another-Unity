using UnityEngine;
using System.Collections.Generic;
using System.IO;


public class AudioManager : MonoBehaviour {

    #region Public Fields

    public static AudioManager instance;

    public int language;

    #endregion

    
    #region Private Fields

    [SerializeField] private AudioSource[] _englishClips; //english audios
    [SerializeField] private AudioSource[] _frenchClips; //french audios
    [SerializeField] private AudioSource[] _portugueseClips; //italian audios
    [SerializeField] private AudioSource[] _spanishClips; //italian audios

    private List<AudioSource[]> _audioClips;

    [SerializeField] private AudioSource _music; //the background music
    [SerializeField] private AudioSource[] _autoModeInstructions; //the audio file played when in automatic mode
    
    private bool _somethingIsPlaying;

    #endregion

    
    #region MonoBehaviour Methods


    private void Awake()
    {
        if (instance == null) instance = this;

        _audioClips = new List<AudioSource[]>();

        //TODO add audio translations as scriptable object configuration to avoid errors with objects not found in scene
        _englishClips = GameObject.Find("EnglishAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_englishClips);

        _frenchClips = GameObject.Find("FrenchAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_frenchClips);

        _portugueseClips = GameObject.Find("PortugueseAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_portugueseClips);
        
        _portugueseClips = GameObject.Find("PortugueseAudios").GetComponentsInChildren<AudioSource>();
        _audioClips.Add(_portugueseClips);
        
        _autoModeInstructions = GameObject.Find("AutoModeInstructions").GetComponentsInChildren<AudioSource>();
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
                if (vKey == UnityEngine.KeyCode.Q)
                    PlaySound(0);
                else if (vKey == UnityEngine.KeyCode.W)
                    PlaySound(5);
                else if (vKey == UnityEngine.KeyCode.E)
                    PlaySound(4);
                else if (vKey == UnityEngine.KeyCode.R)
                    PlaySound(6);
                else if (vKey == UnityEngine.KeyCode.T)
                    PlaySound(7);
                else if (vKey == UnityEngine.KeyCode.Y)
                    PlaySound(1);
                else if (vKey == UnityEngine.KeyCode.U)
                    PlaySound(2);
                else if (vKey == UnityEngine.KeyCode.I)
                    PlaySound(3);
                else if (vKey == UnityEngine.KeyCode.J)
                    PlaySound(8);
                else if (vKey == UnityEngine.KeyCode.K)
                    PlaySound(9);
                else if (vKey == UnityEngine.KeyCode.L)
                    PlaySound(10);
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
    
    public void PlayAudioInstructions(int _selectedInstructions)
    {
        _autoModeInstructions[_selectedInstructions].Play();
    }

    public void StopAudioInstructions()
    {
        foreach(AudioSource _instruction in _autoModeInstructions)
            _instruction.Stop();
    }

    public void PlaySound(int id)
    {
        if (!_somethingIsPlaying)
        {
            Debug.Log("playing sound" + id());
            _audioClips[language][id].Play();
            _music.volume = 0.45f;
        }
    }
    #endregion


    #region Private Methods
    #endregion

}
