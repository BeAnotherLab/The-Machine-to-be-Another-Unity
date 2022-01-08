using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgeQuestion : MonoBehaviour
{
    [SerializeField] private Text _labelText;
    
    public void SetText(float value)
    {
        _labelText.text = Mathf.RoundToInt(value * 100).ToString();
    }
}
