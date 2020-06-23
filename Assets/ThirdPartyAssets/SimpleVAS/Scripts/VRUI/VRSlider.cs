using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;


public class VRSlider : MonoBehaviour {

	private Scrollbar vScale;
	private VRInteractiveItem m_InteractiveItem; 
	private SelectionRadial m_SelectionRadial;

	private Transform reticlePosition;

	private GameObject sliderHandle;

	public float gazeTimeForSelection;
	private float momentOnView;

	private bool lastScrollerStatus;

	// Use this for initialization
	void Start () {

		if (gazeTimeForSelection == 0) gazeTimeForSelection = 1;//defaults to 1
	
		if (this.GetComponent<VRInteractiveItem> () != null) m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();
		else {
			Debug.Log ("Attaching VR Interactive Script to this GameObject, it's required");
			this.gameObject.AddComponent (typeof(VRInteractiveItem));
			m_InteractiveItem = this.GetComponent<VRInteractiveItem> ();
		}

		if(this.GetComponent<BoxCollider>() == null) {
			this.gameObject.AddComponent (typeof(BoxCollider));
			GetComponent<BoxCollider> ().size = new Vector3(this.GetComponent<RectTransform> ().rect.width, this.GetComponent<RectTransform> ().rect.height, 1);
			Debug.Log ("Attaching Box collider to this GameObject, it's required");
		}

		if (this.GetComponent<Scrollbar> () != null) vScale = this.GetComponent<Scrollbar> ();
		else Debug.Log ("No Scrollbar component attached to this GameObject, it's required");

		if (this.GetComponent<Scrollbar>().handleRect.gameObject != null) sliderHandle =  this.GetComponent<Scrollbar>().handleRect.gameObject;
		else Debug.Log ("No child Handle (GameObject) attached to this GameObject, it's required");

		if (Camera.main.gameObject.GetComponent<SelectionRadial>() != null)
			m_SelectionRadial = Camera.main.gameObject.GetComponent<SelectionRadial>();
		else Debug.Log("No SelectionRadial Script attached to the VR Interactive Camera, it's required");

		if (Camera.main.gameObject.GetComponent<SelectionRadial>() != null)
			reticlePosition = Camera.main.gameObject.GetComponent<Reticle>().ReticleTransform;
		else Debug.Log ("No Reticle Script attached to the VR Interactive Camera, it's required with it's references");

		//sliderHandle.SetActive(false);

	}

	// Update is called once per frame
	void Update () {


		if (m_InteractiveItem.IsOver == true) {
			m_SelectionRadial.Show ();

			if (lastScrollerStatus == false)
				momentOnView = Time.realtimeSinceStartup;

			OnScrolling ();
		} 


		if (m_InteractiveItem.IsOver == false && lastScrollerStatus == true)
			m_SelectionRadial.Hide ();

		lastScrollerStatus = m_InteractiveItem.IsOver;


	}

	void OnScrolling () {

		float scrollBarSize = GetComponent<RectTransform>().rect.width;
		float elapsedTime = Time.realtimeSinceStartup - momentOnView;

		Vector3 relativeToCanvas = GetComponentInParent<Canvas>().gameObject.transform.InverseTransformPoint(reticlePosition.transform.position);

		float mappedPosition =(relativeToCanvas.x/(scrollBarSize))+0.5f;          

		if (elapsedTime >= gazeTimeForSelection) 
		{
			sliderHandle.SetActive(true);
			vScale.value = mappedPosition;
			momentOnView = Time.realtimeSinceStartup; //resets to actual time so that the elapsed time goes back to 0
		}
	}
}
