using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaselineManager : MonoBehaviour
{
    [SerializeField] private ExperimentData _experimentData;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BaselineCoroutine());
    }

    public IEnumerator BaselineCoroutine()
    {
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Bitte schauen Sie ein paar Minuten auf das Kreuz.");
        yield return new WaitForSeconds(5);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        yield return new WaitForSeconds(120);
        _experimentData.LoadNextScene();        
    }
}
