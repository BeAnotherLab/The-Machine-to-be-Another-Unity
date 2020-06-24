using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics
{
		
	public class LoadScene : MonoBehaviour {

		public string sceneToLoad;
		public bool changeOnKey;

		// Update is called once per frame
		void Update () 
		{
			if (Input.GetKeyDown ("space") && changeOnKey) OnNextButton ();
		}

		public void OnNextButton () 
		{
			if (sceneToLoad != "") SceneManager.LoadScene (sceneToLoad);
			else SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
		} 
	}

}