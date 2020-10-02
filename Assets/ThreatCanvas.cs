using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreatCanvas : MonoBehaviour
{
    public static ThreatCanvas instance;
    
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private Button _firstThreatButton;
    [SerializeField] private Button _secondThreatButton;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        
        if (!_experimentData.mainComputer)
        {
            _firstThreatButton.gameObject.SetActive(false);
            _secondThreatButton.gameObject.SetActive(false);
        }
        else
        {
            AssignThreatToButton(_experimentData.threatOrder, _firstThreatButton);
        
            if (_experimentData.threatOrder == "Self") AssignThreatToButton("Other", _secondThreatButton);
            else if (_experimentData.threatOrder == "Other") AssignThreatToButton("Self", _secondThreatButton);    
        }
    }

    private void AssignThreatToButton(string threat, Button button)
    {
        button.GetComponentInChildren<Text>().text = "threaten " + threat;
        button.onClick.AddListener(delegate { ThreatManager.instance.StartTask(threat); });
    } 
    
}
