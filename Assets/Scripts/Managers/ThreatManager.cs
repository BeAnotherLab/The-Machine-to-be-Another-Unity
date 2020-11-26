using System;
using System.Collections;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ThreatManager : MonoBehaviour
{
    public static ThreatManager instance;
    
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private PlayableDirector _threatTimeline;
    [SerializeField] private GameObject _tcpConnectionCanvas;
    
    private ThreatOrder _target;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (ThreatCanvas.instance == null) Instantiate(_tcpConnectionCanvas);
    }

    public void StartTask(ThreatOrder target)
    {
        //flip target when sending to other computer
        if (_experimentData.mainComputer && target == ThreatOrder.self)
            OscManager.instance.SendThreatTaskStart(ThreatOrder.other);
        else if (_experimentData.mainComputer && target == ThreatOrder.other)
            OscManager.instance.SendThreatTaskStart(ThreatOrder.self);
            
        _threatTimeline.Play();
        _target = target;
    }

    public void ShowSyncText()
    {
        ThreatCanvas.instance.threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 0.7f;
        ThreatCanvas.instance.threatSyncCanvas.GetComponentInChildren<Text>().text = "Ready?";
    }

    public void HideSyncText()
    {
        ThreatCanvas.instance.threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void Knife()
    {
        ThreatCanvas.instance.threatSyncCanvas.GetComponentInChildren<Text>().text = "Knife " + _target + " !";
        TCPClient.instance.SendTCPMessage(_experimentData.experimentState + "_knife_" + _target);
    }
    
    public void SetText(string text)
    {
        ThreatCanvas.instance.threatSyncCanvas.GetComponentInChildren<Text>().text = text;
    }

}
