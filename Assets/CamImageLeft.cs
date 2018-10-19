﻿using UnityEngine;
using System.Collections;

public class CamImageLeft : MonoBehaviour
{

    private Ovrvision ovrObj = null;

    // Use this for initialization
    void Start()
    {
        ovrObj = GameObject.Find("OvrvisionProCamera").GetComponent<Ovrvision>();
        this.GetComponent<Renderer>().material.mainTexture = ovrObj.GetCameraTextureLeft();
    }

    // Update is called once per frame
    void Update()
    {
        Texture2D left = ovrObj.GetCameraTextureLeft();
        this.GetComponent<Renderer>().material.mainTexture = left;

        //Debug.Log(leftimage.GetPixel(100, 100).ToString()); //Debug
    }
}
