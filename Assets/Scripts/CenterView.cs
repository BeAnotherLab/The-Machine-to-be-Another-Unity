using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class CenterView : MonoBehaviour {

	// Use this for initialization
	void Start () {

		InputTracking.disablePositionalTracking = true;
		//UnityEngine.XR.InputTracking.Recenter ();
	}

	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown("c")) UnityEngine.XR.InputTracking.Recenter();	

	}
}
