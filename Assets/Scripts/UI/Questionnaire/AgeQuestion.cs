using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgeQuestion : MonoBehaviour
{
    [SerializeField] private Text _labelText;
    
    public void ValueUpdated(float value)
    {
        var age = Mathf.RoundToInt(value * 100).ToString();
        _labelText.text = age + " years old";
        GetComponent<ResponseLogger>().SetValue(age);
    }
    
}
