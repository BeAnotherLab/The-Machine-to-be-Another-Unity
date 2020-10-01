using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaselineManager : MonoBehaviour
{
    [SerializeField] private ExperimentData _experimentData;
    [SerializeField] private bool _pre;
    
    // Start is called before the first frame update
    void Start()
    {
        if (_pre) _experimentData.experimentState = ExperimentState.baselinePre;
        else  _experimentData.experimentState = ExperimentState.baselinePost;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
