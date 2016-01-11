using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer UseWebcamTexture;
	public Camera POVCamera;

	private WebCamTexture camTex;
	private float dimLevel = 1;
	private	bool dimmed = false;
	private float dimRate = 0.08f;
	private float zoom = 1;
	private int cameraID = 0;
	private float tiltAngle = 0;

	// Use this for initialization
	void Start () {
		//Debug.Log("Device:" + devices[i].name + " | IS FRONT FACING:" + devices[i].isFrontFacing);
		setCameraID (cameraID);
	}

	public void setDimmed() {		
		dimmed = !dimmed;
	}

	private void setDimLevel() {		
		float next;
		if (dimmed) next = 1;
		else next = 0;
		dimLevel += dimRate * (next - dimLevel);	
		Color c = new Color (dimLevel*255, dimLevel*255, dimLevel*255);
		c = UseWebcamTexture.material.GetColor ("Tint");
		UseWebcamTexture.material.SetColor("Tint", c);
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
			camTex = new WebCamTexture (devices [id].name, 10000, 10000, 60);
			camTex.requestedWidth = 1280;
			camTex.requestedHeight = 1024;
			UseWebcamTexture.material.mainTexture = camTex;
			UseWebcamTexture.material.shader = Shader.Find ("Sprites/Default");
			camTex.Play ();
		}
	}		

	// Update is called once per frame
	void Update () {		
		transform.position = POVCamera.transform.position + POVCamera.transform.forward * 15;
		transform.rotation = POVCamera.transform.rotation; //keep webcam feed in front of head
		transform.rotation *= Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.up); //to adjust for webcam physical orientation
		transform.localScale = new Vector3 (zoom, zoom, 0);
		setDimLevel ();
	}
}
