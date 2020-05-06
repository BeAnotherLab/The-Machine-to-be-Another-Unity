using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    public string key;

    private Text text;
    
    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
    }

    private void Update()
    {
        text.text = LocalizationManager.instance.GetLocalizedValue(key);
    }
}