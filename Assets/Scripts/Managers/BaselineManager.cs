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
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Please look at the cross for a few minutes");
        yield return new WaitForSeconds(5);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        yield return new WaitForSeconds(120);
        _experimentData.LoadNextScene();        
    }
}
