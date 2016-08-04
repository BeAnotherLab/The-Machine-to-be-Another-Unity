using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO.Ports;

public class gui : MonoBehaviour {
	
	public GameObject panel, webCamDisplay, mainCamera;
	private bool monitorGUIEnabled, oculusGUIEnabled;
	private int zoom;
	private int camera_orientation;	
	private int camera_id;
	public Slider pitchSlider, yawSlider, rollSlider, zoomSlider;
	public Dropdown serialDropdown, cameraDropdown;

	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
		setSerialPortDropdownOptions();
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}		

	private void setCameraDropdownOptions(){
		/*Dropdown cameraDropdown = GameObject.Find ("Camera dropdown");
		WebCamDevice[] devices = WebCamTexture.devices;
		for (int i = 0; i < devices.Length; i++) {
			options
		}
		cameraDropdown.AddOptions
		*/
	}

	private void setSerialPortDropdownOptions() {
		string[] ports = SerialPort.GetPortNames ();
		serialDropdown.options.Clear ();
		foreach (string c in ports) {
			serialDropdown.options.Add(new Dropdown.OptionData() {text=c});
		}
		//necessary to refresh the menu
		serialDropdown.value = 0;
		serialDropdown.value = 1;
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
			yawSlider.value = 90 - pitchYawRoll.y + webCamDisplay.GetComponent<arduinoControl>().yawOffset;
			pitchSlider.value = pitchYawRoll.z + 90 + webCamDisplay.GetComponent<arduinoControl>().pitchOffset;
//			pitchOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().pitchOffset;
//			yawOffsetSlider.value = webCamDisplay.GetComponent<arduinoControl>().yawOffset;
			zoomSlider.value =	webCamDisplay.GetComponent<webcam>().zoom;
		}			
	}

}