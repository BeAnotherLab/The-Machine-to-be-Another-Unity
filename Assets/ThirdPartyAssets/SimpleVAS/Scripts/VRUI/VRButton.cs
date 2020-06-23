using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;


public class VRButton: MonoBehaviour {

	public event Action<VRButton> OnButtonSelected;                   // This event is triggered when the selection of the button has finished.		
	
	private SelectionRadial m_SelectionRadial;
	private VRInteractiveItem m_InteractiveItem;

	private Button attachedButton;
	public float gazeTimeForSelection;
	
	private float elapsedSinceGazed,timeAtGaze;

	private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.


	private void OnEnable () {
	
		attachedButton = GetComponent<Button> ();	

		if(this.GetComponent<VRInteractiveItem>() != null) m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();

		else {
			this.gameObject.AddComponent (typeof(VRInteractiveItem));
			Debug.Log ("Attaching VR Interactive Script to this GameObject, it's required");
			m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();
		}

		if(this.GetComponent<BoxCollider>() == null) {
			this.gameObject.AddComponent (typeof(BoxCollider));
			GetComponent<BoxCollider> ().size = new Vector3(this.GetComponent<RectTransform> ().rect.width, this.GetComponent<RectTransform> ().rect.height, 1);
			Debug.Log ("Attaching Box collider to this GameObject, it's required");
		}

		if (Camera.main.gameObject.GetComponent<SelectionRadial>() != null)
			m_SelectionRadial = Camera.main.gameObject.GetComponent<SelectionRadial>();
		else Debug.Log("No SelectionRadial Script attached to the VR Interactive Camera, it's required");

		if (gazeTimeForSelection == 0) gazeTimeForSelection = 1;

		m_InteractiveItem.OnOver += HandleOver;
		m_InteractiveItem.OnOut += HandleOut;
		m_SelectionRadial.OnSelectionComplete += HandleSelectionComplete;

	}

	void Update () {

		if (m_GazeOver && attachedButton.IsInteractable()) 	elapsedSinceGazed = (Time.realtimeSinceStartup - timeAtGaze);

		else if (!m_GazeOver) elapsedSinceGazed = 0;

		if (elapsedSinceGazed >= gazeTimeForSelection) {

			attachedButton.onClick.Invoke (); //"clicks" the button
			attachedButton.interactable = false;

			elapsedSinceGazed = 0; //restart time count

		}
	}	
	

	private void OnDisable () {
		m_InteractiveItem.OnOver -= HandleOver;
		m_InteractiveItem.OnOut -= HandleOut;
		m_SelectionRadial.OnSelectionComplete -= HandleSelectionComplete;

	}


	private void HandleOver() {
		// When the user looks at the rendering of the scene, show the radial.
		if (attachedButton.interactable == true) {
			timeAtGaze = Time.realtimeSinceStartup;
			m_SelectionRadial.Show();
			m_GazeOver = true;
		}
	}


	private void HandleOut()
	{
		// When the user looks away from the rendering of the scene, hide the radial.
		m_SelectionRadial.Hide();

		m_GazeOver = false;
		elapsedSinceGazed = 0;
	}


	private void HandleSelectionComplete() {

	}

}
