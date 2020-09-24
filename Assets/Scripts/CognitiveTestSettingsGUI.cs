using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitiveTestSettingsGUI : MonoBehaviour
{

    public static CognitiveTestSettingsGUI instance;
    
    [SerializeField] private Button _startButton;
    
    private bool _monitorGuiEnabled;
    private float _deltaTime = 0.0f;

    private void Awake()
    {
        if (instance == null) instance = this;        
        
        _startButton.onClick.AddListener(delegate
        {
            CognitiveTestManager.instance.StartInstructions(
                "Him", 
                "none", 
                "Right");            
        });

    }
}
