using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer UseWebcamTexture;
	public Camera POVCamera;
	private WebCamTexture camTex;
	private float dimLevel = 1;
	private	bool dimmed = false;
	private bool headtrackingOn = true;
	private float dimRate = 0.08f;
	public float range = 20;
	public float zoom = 18;
	private int cameraID = 0;
	private float tiltAngle = 0;
	public float width, height; 
	// Use this for initialization
	void Start () {
		//Debug.Log("Device:" + devices[i].name + " | IS FRONT FACING:" + devices[i].isFrontFacing);
		setCameraID (cameraID);
		width = 1920;
		height = 1080;

	}

	public void setDimmed() {		
		dimmed = !dimmed;
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
	}				
		
	public void setZoom(float value) {
		zoom = value;
	}		

	public void setCameraID(int id) {
		WebCamDevice[] devices = WebCamTexture.devices;
		//cameraID = id;
		if (devices.Length >= id - 1) {
			camTex = new WebCamTexture (devices [id].name, (int) width, (int) height, 60);
			camTex.requestedWidth = (int) width;
			camTex.requestedHeight = (int) height;
			UseWebcamTexture.material.mainTexture = camTex;
			UseWebcamTexture.material.shader = Shader.Find ("Sprites/Default");
			camTex.Play ();
		}
	}		

	public void recenterPose(){
		UnityEngine.VR.InputTracking.Recenter();
	}

	public void switchHeadtracking() {
		headtrackingOn = !headtrackingOn;
	}


	public Vector3 toEulerAngles(Quaternion q)
	{
		// Store the Euler angles in radians
		Vector3 pitchYawRoll = new Vector3();

		float sqw = q.w * q.w;
		float sqx = q.x * q.x;
		float sqy = q.y * q.y;
		float sqz = q.z * q.z;

		// If quaternion is normalised the unit is one, otherwise it is the correction factor
		float unit = sqx + sqy + sqz + sqw;
		float test = q.x * q.y + q.z * q.w;

		if (test > 0.4999f * unit)                              // 0.4999f OR 0.5f - EPSILON
		{
			// Singularity at north pole
			pitchYawRoll.y = 2f * (float)Mathf.Atan2(q.x, q.w);  // Yaw
			pitchYawRoll.x = Mathf.PI * 0.5f;                         // Pitch
			pitchYawRoll.z = 0f;                                // Roll
			return pitchYawRoll;
		}
		else if (test < -0.4999f * unit)                        // -0.4999f OR -0.5f + EPSILON
		{
			// Singularity at south pole
			pitchYawRoll.y = -2f * (float)Mathf.Atan2(q.x, q.w); // Yaw
			pitchYawRoll.x = -Mathf.PI * 0.5f;                        // Pitch
			pitchYawRoll.z = 0f;                                // Roll
			return pitchYawRoll;
		}
		else
		{
			pitchYawRoll.y = (float)Mathf.Atan2(2f * q.y * q.w - 2f * q.x * q.z, sqx - sqy - sqz + sqw) * 180/Mathf.PI;       // Yaw
			pitchYawRoll.x = (float)Mathf.Asin(2f * test / unit) * 180/Mathf.PI;                                              // Pitch
			pitchYawRoll.z = (float)Mathf.Atan2(2f * q.x * q.w - 2f * q.y * q.z, -sqx + sqy - sqz + sqw) * 180/Mathf.PI;      // Roll
		}

		return pitchYawRoll;
	}

	// Update is called once per frame
	void Update () {		
		transform.position = POVCamera.transform.position + POVCamera.transform.forward * 15; //keep webcam at a certain distance from head.
		transform.rotation = POVCamera.transform.rotation; //keep webcam feed aligned with head
		transform.rotation *= Quaternion.Euler (0, 0, 1) * Quaternion.AngleAxis(-toEulerAngles(POVCamera.transform.rotation).x, Vector3.forward); //compensate for absence of roll servo
		transform.rotation *= Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.up); //to adjust for webcam physical orientation
		transform.localScale = new Vector3 (width/height*zoom, height/width*zoom, 0);
		setDimLevel ();
	}
}
