using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer[] UseWebcamTexture;
	WebCamTexture camTex;

	public Quaternion baseRotation;
	public float tiltAngle;

	// Use this for initialization
	void Start () {
		WebCamDevice[] devices = WebCamTexture.devices;

		for (int i = 0; i < devices.Length; i++)
		{
			Debug.Log("Device:" + devices[i].name + " | IS FRONT FACING:" + devices[i].isFrontFacing);
		}

		camTex = new WebCamTexture(devices[0].name, 10000, 10000, 60);
		camTex.Play();

		foreach(MeshRenderer r in UseWebcamTexture)
		{
			r.material.mainTexture = camTex;
			Debug.Log("hi");
		}		

		baseRotation = transform.rotation;
		tiltAngle = 0;
	}

	public void setCameraOrientation(){
		tiltAngle += 90;
	}				

	// Update is called once per frame
	void Update () {		
		transform.rotation = Quaternion.Euler (0, 0, tiltAngle) * Quaternion.AngleAxis(camTex.videoRotationAngle, Vector3.up);
	}
}
