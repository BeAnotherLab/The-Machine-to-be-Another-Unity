using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{

    public Material buttonOff, buttonOn;
    public static ConfirmationButtonGraphics instance;

    void Awake()
    {
        if (instance == null) instance = this;
    }


    public void SwitchSelection(bool onOff) {
        if (onOff) {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOn;
            Debug.Log("confirmation on");
            }
        else {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOff;
            Debug.Log("confirmation off");
        }
    }
}
