using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OVRPosition : MonoBehaviour {

    public Quaternion otherPose;
    public Quaternion nextOtherPose;
    public float turningRate = 90f;

    // Use this for initialization
    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        otherPose = Quaternion.RotateTowards(otherPose , nextOtherPose, turningRate * Time.deltaTime);
        
        transform.rotation = Quaternion.Inverse(otherPose); //keep webcam feed aligned with head
    }
}
