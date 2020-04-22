using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IPInputField : MonoBehaviour
{
    
    [SerializeField] private InputField _IPInputField;

    private void Awake()
    {
        _IPInputField.onEndEdit.AddListener(delegate { OscManager.instance.othersIP = _IPInputField.text; });
    }

    private void Start()
    {
        SetIpInputField();
    }

    public void SetIpInputField()
    {
        if (_IPInputField.text != null) _IPInputField.text = PlayerPrefs.GetString("othersIP");
    }    

}
