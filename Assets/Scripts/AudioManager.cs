using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public AudioSource[] germanClips;
	public AudioSource[] frenchClips;
	public AudioSource[] italianClips;
	public AudioSource[] englishClips;

	public AudioSource music;

	private bool somethingIsPlaying = false;

	void Start () {
		music.loop = true;
		music.Play();
	}
	
	// Update is called once per frame
	void Update () {

		CheckAudioPlayback (englishClips);
		if (!somethingIsPlaying) music.volume = 1;

	}
		

	public void PlaySound(string sound){//no language selection
		if (sound == "instructions") StartPlaying (0);
		if (sound == "reminder") StartPlaying (1);
		if (sound == "goodbye") StartPlaying (2);
	}

	private void StartPlaying(int id){//no language selection
		if (!somethingIsPlaying) {
			englishClips [id].Play ();
			music.volume = 0.45f;//this should be fade
		}
	}
		

	private void CheckAudioPlayback(AudioSource[] languageClips){

		somethingIsPlaying = false;

		for (int i = 0; i < languageClips.Length; i++) {
			if (languageClips [i].isPlaying)
				somethingIsPlaying = true;
		}
	}

	public void SelectSound (string language, string sound) {

		AudioClip[] selectedLanguageClips = new AudioClip[3];

		//if (language == "english") selectedLanguageClips = germanClips;
	}
}
