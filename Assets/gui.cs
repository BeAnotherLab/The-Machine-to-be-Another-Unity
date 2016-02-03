using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class gui : MonoBehaviour {
	
	public GameObject panel, webCamDisplay, mainCamera;
	private bool monitorGUIEnabled, oculusGUIEnabled;
	private int zoom;
	private int camera_orientation;	
	private int camera_id;
	public Slider pitchSlider, yawSlider, rollSlider, zoomSlider, pitchOffsetSlider, yawOffsetSlider;

	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}		

	public void centerYaw()  {
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("n")){
			webCamDisplay.GetComponent<webcam>().recenterPose ();
	    }
		else if (Input.GetKeyDown("m")){			
			setMonitorGUIEnabled ();
		}
		panel.SetActive (monitorGUIEnabled);
		if (webCamDisplay.GetComponent<webcam>().isHeadtrackingOn ()) {		
			Vector3 pitchYawRoll = utilities.toEulerAngles (mainCamera.transform.rotation);
			rollSlider.value = pitchYawRoll.x;
			yawSlider.value = 90 - pitchYawRoll.y + webCamDisplay.GetComponent<arduinoControl>().yawOffset;
			pitchSlider.value = pitchYawRoll.z + 90 + webCamDisplay.GetComponent<arduinoControl>().pitchOffset;
//			pitchOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().pitchOffset;
//			yawOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().yawOffset;
			zoomSlider.value =	webCamDisplay.GetComponent<webcam>().zoom;
		}			
	}

}