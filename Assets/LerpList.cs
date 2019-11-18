using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpList : MonoBehaviour
{

    public List<RandomLerpOnAFloat> _lerpList = new List<RandomLerpOnAFloat>(4);

    // Start is called before the first frame update
    void Start()
    {

        
        for(int i = 0; i < _lerpList.Count; i++)
        {
            _lerpList[i] = gameObject.AddComponent<RandomLerpOnAFloat>();
        }
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < _lerpList.Count; i++)
        {
            Debug.Log("item " + i + "is " + _lerpList[i].ChangingValue());
        }
        /*
        Debug.Log("Lerp1 " + _lerpList[0].ChangingValue());
        Debug.Log("Lerp2 " + _lerpList[1].ChangingValue());
        Debug.Log("Lerp3 " + _lerpList[2].ChangingValue());
        Debug.Log("Lerp4 " + _lerpList[3].ChangingValue());*/
    }
}
