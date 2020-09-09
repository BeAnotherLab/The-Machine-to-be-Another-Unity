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
    private GameObject _threatSyncCanvas;
    private string _target; //TODO counterbalance
    
    private void Awake()
    {
        if (instance == null) instance = this;
        _threatSyncCanvas = GameObject.Find("ThreatSyncCanvas");
    }

    public void StartTask(string target)
    {
        //flip target when sending to other computer
        if (_experimentData.mainComputer && target == "Self")
            OscManager.instance.SendThreatTaskStart("Other");
        else if (_experimentData.mainComputer && target == "Other")
            OscManager.instance.SendThreatTaskStart("Self");
            
        VideoFeed.instance.SetDimmed(false);
        
        _threatTimeline.Play();
        _target = target;
    }

    public void ShowSyncText()
    {
        _threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 0.7f;
        _threatSyncCanvas.GetComponentInChildren<Text>().text = "Ready?";
    }

    public void HideSyncText()
    {
        _threatSyncCanvas.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void Knife()
    {
        _threatSyncCanvas.GetComponentInChildren<Text>().text = "Knife " + _target + " !";
        TCPClient.instance.SendTCPMessage(_experimentData.experimentState + " knife " + _target);
        StartCoroutine(WaitAndDim());
    }
    
    public void SetText(string text)
    {
        _threatSyncCanvas.GetComponentInChildren<Text>().text = text;
    }

    private IEnumerator WaitAndDim()
    {
        yield return new WaitForSeconds(6f);
        VideoFeed.instance.SetDimmed(true);
    }
}
