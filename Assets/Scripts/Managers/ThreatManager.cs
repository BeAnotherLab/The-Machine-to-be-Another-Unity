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
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _threatSyncCanvas = GameObject.Find("ThreatSyncCanvas");
    }

    public void StartTask()
    {
        if (_experimentData.mainComputer)
            OscManager.instance.SendThreatTaskStart();
        
        _threatTimeline.Play();
    }

    public void ShowSyncText(bool show)
    {
        _threatSyncCanvas.GetComponent<CanvasGroup>().alpha = show ? 1 : 0;
    }
    
    public void SetText(string text)
    {
        _threatSyncCanvas.GetComponentInChildren<Text>().text = text;
    }
}
