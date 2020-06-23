using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics
{
	
public class LoadScene : MonoBehaviour {

	public string sceneToLoad;
	public bool changeOnKey;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
		if (changeOnKey)
			if (Input.GetKeyDown ("space"))
				OnNextButton ();
	}


	public void OnNextButton () {

		if (sceneToLoad != "")
			SceneManager.LoadScene (sceneToLoad);
		else
			SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
	} 
}

}