using UnityEngine;
using System.Collections;

public class gui : MonoBehaviour {
	
	public GameObject panel;
	private bool monitorGUIEnabled, oculusGUIEnabled;
	private float dimLevel;
	private bool dimmed;
	private int zoom;
	private float pitch_offset, yaw_offset;
	private int camera_orientation;	
	private int camera_id;


	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}

	public void setDimLevel() {		
	}

	public void setDimmed() {
		
	}

	public void setZoom() {
	}

	public void centerYaw()  {
	}

	public void setCameraID(int id) {
	}



	public void setPitchOffset() {
	}

	public void setYawOffset() {
	}

	public void setPitch(){
	}

	public void setYaw() {
	}

	// Update is called once per frame
	void Update () {
		panel.SetActive (monitorGUIEnabled);

	}

}
