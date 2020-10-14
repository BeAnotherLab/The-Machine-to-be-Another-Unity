using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaselineManager : MonoBehaviour
{
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private GameObject _tcpConnectionCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BaselineCoroutine());
        if (ThreatCanvas.instance == null) Instantiate(_tcpConnectionCanvas);
    }

    public IEnumerator BaselineCoroutine()
    {
        TCPClient.instance.SendTCPMessage("baseline_start");
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Bitte schauen Sie auf das Kreuz, entspannen Sie sich und bewegen Sie sich nicht. Wir messen ihre Hirnaktivität für ein paar Minuten.");
        yield return new WaitForSeconds(10);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        yield return new WaitForSeconds(120);
        TCPClient.instance.SendTCPMessage("baseline_end");
        yield return new WaitForSeconds(1);
        _experimentData.LoadNextScene();        
    }
}
