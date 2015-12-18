using UnityEngine;
using System.Collections;

public class webcam : MonoBehaviour {

	public MeshRenderer UseWebcamTexture;
	private Shader shader;
	private WebCamTexture camTex;
	private float dimLevel;
	private bool dimmed;
	private float dimRate = 0.008f;

	// Use this for initialization
	void Start () {
		WebCamDevice[] devices = WebCamTexture.devices;

		camTex = new WebCamTexture(devices[0].name, 10000, 10000, 60);
		camTex.requestedWidth = 1280;
		camTex.requestedHeight = 1024;
		camTex.Play();

		UseWebcamTexture.material.mainTexture = camTex;
		UseWebcamTexture.material.shader = Shader.Find ("Sprites/Default");
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

	// Update is called once per frame
	void Update () {
		setDimLevel ();
	}
}
