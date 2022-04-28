using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VRStandardAssets.Utils;


public class CustomVRButton: MonoBehaviour {
	
	private CustomSelectionRadial m_SelectionRadial;
	private VRInteractiveItem m_InteractiveItem;

	public float gazeTimeForSelection;
	
	private float elapsedSinceGazed,timeAtGaze;

	private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.

	private void OnEnable () {
	
		if(GetComponent<VRInteractiveItem>() != null) m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();

		else {
			gameObject.AddComponent (typeof(VRInteractiveItem));
			Debug.Log ("Attaching VR Interactive Script to this GameObject, it's required");
			m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();
		}

		if(GetComponent<BoxCollider>() == null) {
			gameObject.AddComponent (typeof(BoxCollider));
			GetComponent<BoxCollider> ().size = new Vector3(GetComponent<RectTransform> ().rect.width, GetComponent<RectTransform> ().rect.height, 1);
			Debug.Log ("Attaching Box collider to this GameObject, it's required");
		}

		if (Camera.main.gameObject.GetComponent<CustomSelectionRadial>() != null)
			m_SelectionRadial = Camera.main.gameObject.GetComponent<CustomSelectionRadial>();
		else Debug.Log("No SelectionRadial Script attached to the VR Interactive Camera, it's required");

		if (gazeTimeForSelection == 0) gazeTimeForSelection = 1;

		m_InteractiveItem.OnOver += HandleOver;
		m_InteractiveItem.OnOut += HandleOut;
	}

	private void OnDisable () {
		m_InteractiveItem.OnOver -= HandleOver;
		m_InteractiveItem.OnOut -= HandleOut;
	}
	
	public void HandleSelectionComplete() {
		if (m_GazeOver) {
			//raise event
		}
		HandleOut(); //necessary?
	}
	
	private void HandleOver() {
		// When the user looks at the rendering of the scene, show the radial.
		if (XRDevice.userPresence == UserPresenceState.Present)
		{
			m_SelectionRadial.Show();
            m_GazeOver = true;
            //maybe animate button somehow here
		}
	}

	private void HandleOut()
	{
		// When the user looks away from the rendering of the scene, hide the radial.
		m_SelectionRadial.Hide();

		m_GazeOver = false;
	}
}
