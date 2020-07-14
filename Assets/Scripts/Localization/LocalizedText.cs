using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    private Text text;
    
    // Use this for initialization
    void Start()
    {
        text = GetComponent<Text>();
    }

    public void SetTextFromKey(string key)
    {
        text.text = LocalizationManager.instance.GetLocalizedValue(key);
    }
}