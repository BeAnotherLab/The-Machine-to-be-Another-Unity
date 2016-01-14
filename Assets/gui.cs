using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gui : MonoBehaviour {
	
	public GameObject panel, webCamDisplay, mainCamera;
	private bool monitorGUIEnabled, oculusGUIEnabled;
	private int zoom;
	private float pitch_offset, yaw_offset;
	private float pitch, yaw;
	private int camera_orientation;	
	private int camera_id;
	public Slider pitchSlider, yawSlider, rollSlider, zoomSlider;

	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}		

	public void centerYaw()  {
	}

	public void setPitchOffset() {
	}

	public void setYawOffset() {
	}		

	// Update is called once per frame
	void Update () {
		panel.SetActive (monitorGUIEnabled);
		if (webCamDisplay.GetComponent<webcam>().isHeadtrackingOn ()) {
			pitchSlider.value = mainCamera.transform.eulerAngles.x;
			yawSlider.value = mainCamera.transform.eulerAngles.y;
		}
		//rollSlider.value = mainCamera.transform.eulerAngles.z; 		
	}

}
 