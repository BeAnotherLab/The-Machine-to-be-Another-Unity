using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer[] UseWebcamTexture;
	WebCamTexture camTex;

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

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
