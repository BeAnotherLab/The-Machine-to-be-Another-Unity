using UnityEngine;
using System.Collections;

public class CamImageRight : MonoBehaviour
{

    private Ovrvision ovrObj = null;

    // Use this for initialization
    void Start()
    {
        ovrObj = GameObject.Find("OvrvisionProCamera").GetComponent<Ovrvision>();
        this.GetComponent<Renderer>().material.mainTexture = ovrObj.GetCameraTextureRight();
    }

    // Update is called once per frame
    void Update()
    {
        Texture2D right = ovrObj.GetCameraTextureRight();
        this.GetComponent<Renderer>().material.mainTexture = right;

        //Debug.Log(leftimage.GetPixel(100, 100).ToString()); //Debug
    }
}
