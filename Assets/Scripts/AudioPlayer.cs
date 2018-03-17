using UnityEngine;
using System.Collections;
using System.IO;


public class AudioPlayer : MonoBehaviour {

	public AudioSource[] clips;
	public AudioSource music;


	private bool somethingIsPlaying;

	// Use this for initialization
	void Start () {
		//play looping background music
		music.loop = true;
		music.Play();
		foreach (AudioSource clip in clips) {
			clip.Pause ();
		}
	}
	
	// Update is called once per frame
	void Update () {

		foreach(KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))){
			if(Input.GetKey(vKey)){
				//your code here
				if (vKey == UnityEngine.KeyCode.Q)
					playSound (0);
				else if (vKey == UnityEngine.KeyCode.W)
					playSound (1);
				else if (vKey == UnityEngine.KeyCode.E)
					playSound (2);
				else if (vKey == UnityEngine.KeyCode.R)
					playSound (3);
				else if (vKey == UnityEngine.KeyCode.T)
					playSound (4);
				else if (vKey == UnityEngine.KeyCode.Y)
					playSound (5);
				else if (vKey == UnityEngine.KeyCode.U)
					playSound (6);
				else if (vKey == UnityEngine.KeyCode.I)
					playSound (7);
			}
		}

		somethingIsPlaying = false;

		//check if some audio is playing 
		for (int i = 0; i < clips.Length; i++) {
			if (clips [i].isPlaying)
				somethingIsPlaying = true;
		}

		if (!somethingIsPlaying)
			music.volume = 1;
				
	}

	public void playSound(int id){
		if (!somethingIsPlaying) {
			Debug.Log ("playing sound" + id.ToString ());
			clips [id].Play ();
			music.volume = 0.45f;
		}
	}

	/*
	 * */
//assign a TouchOSC control
//assign a keyboard key for each sound
//show on GUI what key triggers what sound
//prevent from playing if one is already playing



}
