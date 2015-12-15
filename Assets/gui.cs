using UnityEngine;
using System.Collections;

public class gui : MonoBehaviour {
	
	public GameObject panel;
	private bool monitorGUIEnabled;

	// Use this for initialization
	void Start () {
		monitorGUIEnabled = true;
	}

	public void setMonitorGUIEnabled() {
		monitorGUIEnabled = !monitorGUIEnabled;
	}

	// Update is called once per frame
	void Update () {
		panel.SetActive (monitorGUIEnabled);
		/*for (int i = 0; i < canvas.childCount; i++) 
		{
			
		}*/
		//canvas.chi
	}

}
