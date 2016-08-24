﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;

public class gui : MonoBehaviour {
	
	public GameObject panel, webCamDisplay, mainCamera;
	public Slider pitchSlider, yawSlider, rollSlider, zoomSlider;
	public Dropdown serialDropdown, cameraDropdown;

	private bool twoWaySwap = true;
	private bool monitorGUIEnabled, oculusGUIEnabled;
	private int zoom;
	private int camera_orientation;	
	private int camera_id;
	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
		setCameraDropdownOptions ();
		setSerialPortDropdownOptions();
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}		

	private void setSerialPortDropdownOptions() {
		if (!twoWaySwap) {
			string[] ports = SerialPort.GetPortNames ();
			serialDropdown.options.Clear ();
			foreach (string c in ports) {
				serialDropdown.options.Add (new Dropdown.OptionData () { text = c });
			}
			serialDropdown.value = PlayerPrefs.GetInt ("Serial port");
		}
	}

	private void setCameraDropdownOptions(){
		WebCamDevice[] devices = WebCamTexture.devices;
		cameraDropdown.options.Clear ();
		foreach(WebCamDevice device in devices){
			cameraDropdown.options.Add (new Dropdown.OptionData () { text = device.name });			
		}
		cameraDropdown.value = PlayerPrefs.GetInt ("cameraID");

	}
		

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("b")){
			webCamDisplay.GetComponent<webcam>().setDimmed();
		}
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
			yawSlider.value = 90 - pitchYawRoll.y;
			pitchSlider.value = pitchYawRoll.z + 90;
//			pitchOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().pitchOffset;
//			yawOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().yawOffset;
			zoomSlider.value =	webCamDisplay.GetComponent<webcam>().zoom;
		}			
		//cameraDropdown.RefreshShownValue();
	

	}

}