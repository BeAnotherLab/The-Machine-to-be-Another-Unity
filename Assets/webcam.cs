using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer UseWebcamTexture;
	private Shader shader;
	private WebCamTexture camTex;
	private float dimLevel;
	private bool dimmed;
	private float dimRate = 0.008f;
	public float zoom = 1;
	private int cameraID = 0;
	public Quaternion baseRotation;
	public float tiltAngle = 0;

	// Use this for initialization
	void Start () {
		//Debug.Log("Device:" + devices[i].name + " | IS FRONT FACING:" + devices[i].isFrontFacing);

		setCameraID (cameraID);



		baseRotation = transform.rotation;

	}

	public void setDimmed() {		
		dimmed = !dimmed;
	}

	private void setDimLevel() {		
		float next;
		if (dimmed) next = 1;
		else next = 0;
		dimLevel += dimRate * (dimLevel - next);	
		UseWebcamTexture.material.SetColor("_Tint", new Color(next,next,next));
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
		transform.rotation = Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.up);
		transform.localScale = new Vector3 ((zoom+0.15f)*20, (zoom+0.15f)*20, 0);
		setDimLevel ();
	}
}
