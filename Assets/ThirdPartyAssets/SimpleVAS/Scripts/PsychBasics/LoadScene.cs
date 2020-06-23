using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityPsychBasics
{
	
    public class LoadScene : MonoBehaviour {

        [SerializeField]
        public string sceneToLoad;
        [SerializeField]
        public bool changeOnKey, changeAtTime;
        [SerializeField]
        public float sceneDuration;
	
	    // Update is called once per frame
	    void Update () {

            if (changeAtTime)
                StartCoroutine(ChangeAtTime(sceneDuration));

		    if (changeOnKey)
			    if (Input.GetKeyDown ("space"))
				    Load();
	    }


	    public void Load() {

		    if (sceneToLoad != "")
			    SceneManager.LoadScene (sceneToLoad);
		    else
			    SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
	    } 

        private IEnumerator ChangeAtTime(float _time){
            yield return new WaitForFixedTime(_time);
            Load();
        }
    }

}