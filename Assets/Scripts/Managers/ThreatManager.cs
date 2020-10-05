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
    
    private string _target; //TODO counterbalance
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        if (ThreatCanvas.instance == null) Instantiate(_tcpConnectionCanvas);
    }

    public void StartTask(string target)
    {
        //flip target when sending to other computer
        if (_experimentData.mainComputer && target == "Self")
            OscManager.instance.SendThreatTaskStart("Other");
        else if (_experimentData.mainComputer && target == "Other")
            OscManager.instance.SendThreatTaskStart("Self");
            
        _threatTimeline.Play();
        _target = target;
    }

    public void ShowSyncText()
    {
        ThreatCanvas.instance.gameObject.GetComponent<CanvasGroup>().alpha = 0.7f;
        ThreatCanvas.instance.gameObject.GetComponentInChildren<Text>().text = "Ready?";
    }

    public void HideSyncText()
    {
        ThreatCanvas.instance.gameObject.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void Knife()
    {
        ThreatCanvas.instance.gameObject.GetComponentInChildren<Text>().text = "Knife " + _target + " !";
        TCPClient.instance.SendTCPMessage(_experimentData.experimentState + " knife " + _target);
    }
    
    public void SetText(string text)
    {
        ThreatCanvas.instance.gameObject.GetComponentInChildren<Text>().text = text;
    }

}
