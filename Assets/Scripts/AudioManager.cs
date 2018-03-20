using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public AudioSource[] germanClips;
	public AudioSource[] frenchClips;
	public AudioSource[] italianClips;
	public AudioSource[] englishClips;

	public AudioSource music;

	private AudioSource[] selectedLanguage;
	private string previouslySelectedLanguage;

	private bool somethingIsPlaying = false;

	void Start () {
		music.loop = true;
		music.Play();
		selectedLanguage = englishClips;
	}
	
	// Update is called once per frame
	void Update () {

		CheckAudioPlayback ();
		if (!somethingIsPlaying) music.volume = 1;

		if (previouslySelectedLanguage != LanguageTextDictionary.selectedLanguage)
			SelectAudioLanguage (LanguageTextDictionary.selectedLanguage);

		previouslySelectedLanguage = LanguageTextDictionary.selectedLanguage;
	}

	public void SelectAudioLanguage (string language){
		if (language == "german")	selectedLanguage = germanClips;
		if (language == "french")	selectedLanguage = frenchClips;
		if (language == "italian")	selectedLanguage = italianClips;
		if (language == "english")	selectedLanguage = englishClips;
	}
		

	public void PlaySound(string sound){//no language selection
		
		if (!somethingIsPlaying) {
			if (sound == "instructions") StartPlaying (0);
			if (sound == "reminder") StartPlaying (1);
			if (sound == "hands") StartPlaying (2);
			if (sound == "object") StartPlaying (3);
			if (sound == "mirror") StartPlaying (4);
			if (sound == "goodbye") StartPlaying (5);
		}

		else if (somethingIsPlaying) Debug.Log("Could not play sound " + sound + " because another sound is playing");
	}

	private void StartPlaying(int id){//no language selection
		if (!somethingIsPlaying) {
			englishClips [id].Play ();
			music.volume = 0.45f;//this should be fade
		}
	}
		

	private void CheckAudioPlayback(){

		somethingIsPlaying = false;

		for (int i = 0; i < selectedLanguage.Length; i++) {
			if (selectedLanguage [i].isPlaying)	somethingIsPlaying = true;
		}
	}

	public void StopAll() {
		if (somethingIsPlaying) {
			for (int i = 0; i < selectedLanguage.Length; i++) {
				selectedLanguage [i].Stop ();
				music.volume = 1f;
			}
		}
	}

	public void SelectSound (string language, string sound) {

		AudioClip[] selectedLanguageClips = new AudioClip[3];

		//if (language == "english") selectedLanguageClips = germanClips;
	}
}
