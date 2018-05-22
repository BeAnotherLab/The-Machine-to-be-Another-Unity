using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer UseWebcamTexture;
	public Camera POVCamera;
	public float range = 20;
	public float zoom = 39.5f;
	public float widthHeightRatio = 1.25f;
	public int cameraID = 1;
	public float width, height; 
	public Quaternion otherPose;
	public Quaternion nextOtherPose;
	public Vector3 otherPosition;
	public float turningRate = 90f;

	private float tiltAngle = 0;
	private WebCamTexture camTex;
	public float dimLevel = 1;
	private	bool dimmed = false;
	private bool headtrackingOn = true;
	private float dimRate = 0.08f;
	private bool twoWaySwap = true;

	void Start () {

		WebCamDevice[] devices = WebCamTexture.devices;
		string deviceName = devices[cameraID].name;
		camTex = new WebCamTexture (deviceName, 1920, 1080);//, 1920, 1080, FPS); //PERFORMANCE DEPENDS ON FRAMERATE AND RESOLUTION
		camTex.Play();

		recenterPose ();
		otherPose = new Quaternion ();
		otherPosition = new Vector3 ();

	}

	/*
	void getPlayerPrefs() {
		tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
		zoom = PlayerPrefs.GetFloat ("zoom");
        if (zoom == 0)
        {
            zoom = 39.5f;
            PlayerPrefs.SetFloat("zoom", 39.5f);
        }
		cameraID = PlayerPrefs.GetInt ("cameraID");
	}*/

	public void setDimmed() {		
		dimmed = !dimmed;
	}

	public void setDimmed(bool dim) {		
		dimmed = dim;
	}

	public bool isHeadtrackingOn() {
		return headtrackingOn;
	}

	private void setDimLevel() {		
		float next;
		if (dimmed) next = 1;
		else next = 0;
		dimLevel += dimRate * (next - dimLevel);	
		Color c = new Color (dimLevel*range, dimLevel*range, dimLevel*range);
		UseWebcamTexture.material.SetColor("_Color", c);
	}

	public void setCameraOrientation(){
		tiltAngle += 90;
		PlayerPrefs.SetFloat ("tiltAngle", tiltAngle);
	}				
		
	public void setZoom(float value) {
		zoom = value;
		PlayerPrefs.SetFloat ("zoom", zoom);
	}		
		
		
	public void recenterPose(){
		UnityEngine.XR.InputTracking.Recenter();
	}

	public void switchHeadtracking() {
		headtrackingOn = !headtrackingOn;		
	}
		
	// Update is called once per frame
	void Update () {	
		// Turn towards our target rotation.
		otherPose = Quaternion.RotateTowards(otherPose, nextOtherPose, turningRate * Time.deltaTime);

		if (Input.GetKeyDown ("b")) setDimmed ();

		if (!twoWaySwap) {
			transform.position = POVCamera.transform.position + POVCamera.transform.forward * 35; //keep webcam at a certain distance from head.
			transform.rotation = POVCamera.transform.rotation; //keep webcam feed aligned with head
			transform.rotation *= Quaternion.Euler (0, 0, 1) * Quaternion.AngleAxis (-utilities.toEulerAngles (POVCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
			transform.rotation *= Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis (camTex.videoRotationAngle, Vector3.up); //to adjust for webcam physical orientation
			//transform.localScale = new Vector3 (width/height*zoom, height/width*zoom, 0);
			//Arthurs: transform.localScale = new Vector3 (widthHeightRatio * zoom, 1 / widthHeightRatio * zoom, 1 * zoom);
			transform.localScale = new Vector3 (0.9f, 1, -1); 
		} else {
			//transform.position = otherPosition + otherPose * Vector3.forward * 35; //keep webcam at a certain distance from head.
			transform.rotation = otherPose; //keep webcam feed aligned with head
			//transform.rotation *= Quaternion.Euler (0, 0, 1) * Quaternion.AngleAxis (-utilities.toEulerAngles (POVCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
			//transform.rotation *= Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis (camTex.videoRotationAngle, Vector3.up); //to adjust for webcam physical orientation
			//transform.localScale = new Vector3 (width/height*zoom, height/width*zoom, 0);
			//transform.localScale = new Vector3 (widthHeightRatio * zoom, 1 / widthHeightRatio * zoom, 1 * zoom); 
			transform.localScale = new Vector3 (0.9f, 1, -1); 
		}

		UseWebcamTexture.material.mainTexture = camTex;
		setDimLevel ();
	}

	void OnDestroy(){
		camTex.Stop();
		PlayerPrefs.SetInt ("cameraID", cameraID);
	}
}
