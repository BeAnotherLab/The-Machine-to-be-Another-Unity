using UnityEngine;
using System.Collections;
using System.IO;


public class AudioPlayer : MonoBehaviour {

    #region Public Fields

    public static AudioPlayer instance;

    #endregion

    #region Private Fields
    [SerializeField]
    private AudioSource[] _clips; //audios to be triggered with keys or touchOSC
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

        _clips = transform.Find("English audios").GetComponentsInChildren<AudioSource>();
    }

    // Use this for initialization
    private void Start()
    {
        //play looping background music
        _music.loop = true;
        _music.Play();
        foreach (AudioSource clip in _clips)
            clip.Pause();
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(vKey))
            {
                //your code here
                if (vKey == UnityEngine.KeyCode.Q)
                    playSound(0);
                else if (vKey == UnityEngine.KeyCode.W)
                    playSound(1);
                else if (vKey == UnityEngine.KeyCode.E)
                    playSound(2);
                else if (vKey == UnityEngine.KeyCode.R)
                    playSound(3);
                else if (vKey == UnityEngine.KeyCode.T)
                    playSound(4);
                else if (vKey == UnityEngine.KeyCode.Y)
                    playSound(5);
                else if (vKey == UnityEngine.KeyCode.U)
                    playSound(6);
                else if (vKey == UnityEngine.KeyCode.I)
                    playSound(7);
            }
        }

        _somethingIsPlaying = false;

        //check if some audio is playing 
        for (int i = 0; i < _clips.Length; i++)
        {
            if (_clips[i].isPlaying)
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
            _clips[id].Play();
            _music.volume = 0.45f;
        }
    }
    #endregion


    #region Private Methods
    #endregion

}
