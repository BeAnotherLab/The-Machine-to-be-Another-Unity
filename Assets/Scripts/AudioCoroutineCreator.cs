using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCoroutineCreator : MonoBehaviour {

	public List<float> audioTime;
	public AudioSource[] audios;

	private int itemCounter;

	//is called from elsewhere to start audio coroutines, one for each item in the list of times, which should match the number of audios.
	public void StartAudioCoroutines() {
		
		foreach (var item in audioTime) {
			Debug.Log (item);
			StartCoroutine (coroutineTest (itemCounter, item));
			itemCounter++;
			//Debug.Log (itemCounter);
		}

	}

	//is called from elsewhere to start audio coroutines
	public void StopAudioCoroutines(){
		StopAllCoroutines ();
	}


	public IEnumerator coroutineTest(int audioIndex, float time) {
		yield return new WaitForFixedTime (time);
		audios [audioIndex].Play ();
	}

}
