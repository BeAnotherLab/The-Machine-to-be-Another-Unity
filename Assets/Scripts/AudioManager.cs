using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

    #region Public Fields

    public AudioSource[] germanClips;
    public AudioSource[] frenchClips;
    public AudioSource[] italianClips;
    public AudioSource[] englishClips;
    public AudioSource[] triggerableAudios;

    public AudioSource music;

    #endregion

    #region Private Fields

    private AudioSource[] _selectedLanguage;
    private string _previouslySelectedLanguage;
    private bool _somethingIsPlaying = false; //

    #endregion

    #region MonoBehaviour Methods

    private void Start()
    {
        music.loop = true;
        music.Play();
        DontDestroyOnLoad(this.gameObject);

        _selectedLanguage = englishClips;
    }

    // Update is called once per frame
    private void Update()
    {

        CheckAudioPlayback();
        //	if (!somethingIsPlaying) music.volume = 0.75f;

        if (_previouslySelectedLanguage != LanguageTextDictionary.selectedLanguage)
            SelectAudioLanguage(LanguageTextDictionary.selectedLanguage);

        _previouslySelectedLanguage = LanguageTextDictionary.selectedLanguage;

        for (int i = 0; i < triggerableAudios.Length; i++)
        {
            if (Input.GetKeyDown(i.ToString())) PlayTriggerableSound(i);
        }
    }
    #endregion

    #region Public Methods

    public void SelectAudioLanguage(string language)
    {
        if (language == "german") _selectedLanguage = germanClips;
        if (language == "french") _selectedLanguage = frenchClips;
        if (language == "italian") _selectedLanguage = italianClips;
        if (language == "english") _selectedLanguage = englishClips;
    }

    public void PlayTriggerableSound(int id)
    {
        triggerableAudios[id].Play();
    }

    public void PlaySound(string sound)
    {//no language selection

        if (!_somethingIsPlaying)
        {
            if (sound == "instructions") StartPlaying(0);
            if (sound == "reminder") StartPlaying(1);
            if (sound == "hands") StartPlaying(2);
            if (sound == "object") StartPlaying(3);
            if (sound == "mirror") StartPlaying(4);
            if (sound == "goodbye") StartPlaying(5);
        }

        else if (_somethingIsPlaying) Debug.Log("Could not play sound " + sound + " because another sound is playing");
    }

    public void StopAll()
    {
        if (_somethingIsPlaying)
        {
            for (int i = 0; i < _selectedLanguage.Length; i++)
            {
                _selectedLanguage[i].Stop();
                //music.volume = 0.75f;
            }
        }
    }

    public void SelectSound(string language, string sound)
    {

        AudioClip[] selectedLanguageClips = new AudioClip[3];

        //if (language == "english") selectedLanguageClips = germanClips;
    }

    #endregion

    #region Private Methods

    private void StartPlaying(int id)
    {//no language selection
        if (!_somethingIsPlaying)
        {
            englishClips[id].Play();
            //music.volume = 0.45f;//this should be fade
        }
    }

    private void CheckAudioPlayback()
    {

        _somethingIsPlaying = false;

        for (int i = 0; i < _selectedLanguage.Length; i++)
        {
            if (_selectedLanguage[i].isPlaying) _somethingIsPlaying = true;
        }

        for (int i = 0; i < triggerableAudios.Length; i++)
        {
            if (triggerableAudios[i].isPlaying) _somethingIsPlaying = true;
        }
    }

    #endregion

    
}
