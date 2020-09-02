using System.Net.Mime;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ThreatManager : MonoBehaviour
{
    public static ThreatManager instance;
    
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private PlayableDirector _threatTimeline;
    private GameObject _threatSyncCanvas;
    private string _target; 
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _threatSyncCanvas = GameObject.Find("ThreatSyncCanvas");
    }

    public void StartTask(string target)
    {
        if (_experimentData.mainComputer) OscManager.instance.SendThreatTaskStart(target); //trigger threat task on other computer
        _threatTimeline.Play();
        _target = target;
    }

    public void ShowSyncText()
    {
        _threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 1;
        _threatSyncCanvas.GetComponentInChildren<Text>().text = "Ready?";
    }

    public void HideSyncText()
    {
        _threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void Knife()
    {
        _threatSyncCanvas.GetComponentInChildren<Text>().text = "Knife!";
        TCPClient.instance.SendTCPMessage("knife " + _target);
    }
    
    public void SetText(string text)
    {
        _threatSyncCanvas.GetComponentInChildren<Text>().text = text;
    }
}
