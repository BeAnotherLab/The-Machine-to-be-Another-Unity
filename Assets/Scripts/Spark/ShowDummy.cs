using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDummy : MonoBehaviour
{

    public static ShowDummy instance;
    // Start is called before the first frame update
    private  void Awake()
    {
        if (instance == null) instance = this;
    }

    public void Show(bool show)
    {
        GetComponent<MeshRenderer>().enabled = show;
    }
}
