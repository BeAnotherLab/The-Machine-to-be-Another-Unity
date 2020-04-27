using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public abstract class TestManager : MonoBehaviour
{
    protected string _subjectID;
    protected string _prepost;

    //the timer to measure reaction time
    protected Stopwatch _timer;
    protected Coroutine _trialCoroutine;
    protected string _filePath;

    //for parsing the trial structure JSONs
    protected JSONObject _finalTrialsList;
    protected int _trialIndex;
    
    //The different steps in our test
    public enum steps { init, instructions, practice, testing };
    protected steps _currentStep;
    
    
    //flag to define the time frame in which we accept answers
    protected bool _waitingForAnswer;
    
    // Start is called before the first frame update
    protected void Start()
    {
        _currentStep = steps.init;
        _timer = new Stopwatch();
        _finalTrialsList = new JSONObject();
    }
    
    protected void FinishTest()
    {
        _trialIndex = 0;
        InstructionsTextBehavior.instance.ShowInstructionText("Ok, the test is now finished! We will proceed with the next step now", 3);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
