using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTrigerrer : MonoBehaviour {

	public AudioSource[] englishClips;

	private bool somethingIsPlaying = false;

	void Start () {
	}
	/*
	// Update is called once per frame
	void Update () {

		CheckAudioPlayback ();

		if (previouslySelectedLanguage != LanguageTextDictionary.selectedLanguage)
			SelectAudioLanguage (LanguageTextDictionary.selectedLanguage);

		previouslySelectedLanguage = LanguageTextDictionary.selectedLanguage;

		for (int i = 0; i < englishClips.Length;
	}

	public void SelectAudioLanguage (string language){
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
		}
	}
		

	private void CheckAudioPlayback(){

		somethingIsPlaying = false;

		for (int i = 0; i < englishClips.Length; i++) {
			if (englishClips [i].isPlaying)	somethingIsPlaying = true;
		}
	}

	public void StopAll() {
		if (somethingIsPlaying) {
			for (int i = 0; i < englishClips.Length; i++) {
				englishClips [i].Stop ();
			}
		}
	}

	public void SelectSound (string language, string sound) {

		AudioClip[] selectedLanguageClips = new AudioClip[3];

		//if (language == "english") selectedLanguageClips = germanClips;
	}*/
}
