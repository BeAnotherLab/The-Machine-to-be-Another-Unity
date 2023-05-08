using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// THIS SCRIPT IS ONLY USED FOR TESTING WITHOUT HEADSET
/// </summary>
public class MoveHead : MonoBehaviour
{

    private Transform head;
    private float[] sides = new float[] { 60, 290};

    // Start is called before the first frame update
    void Start()
    {
        head = this.gameObject.transform;   
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            Vector3 _rot = new Vector3(0, sides[Random.Range(0, 2)], 0);
            head.rotation = Quaternion.Euler(_rot);
        }
    }
}
