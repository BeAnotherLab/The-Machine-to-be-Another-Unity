using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreatCanvas : MonoBehaviour
{
    public static ThreatCanvas instance;
    public GameObject threatSyncCanvas;

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
        
            if (_experimentData.threatOrder == ThreatOrder.self) AssignThreatToButton(ThreatOrder.other, _secondThreatButton);
            else if (_experimentData.threatOrder == ThreatOrder.other) AssignThreatToButton(ThreatOrder.self, _secondThreatButton);    
        }
    }

    private void AssignThreatToButton(ThreatOrder threat, Button button)
    {
        button.GetComponentInChildren<Text>().text = "threaten " + threat;
        button.onClick.AddListener(delegate { ThreatManager.instance.StartTask(threat); });
    } 
    
}
