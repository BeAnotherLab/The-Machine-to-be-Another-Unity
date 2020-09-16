using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public abstract class TestManager : MonoBehaviour
{
    //the timer to measure reaction time
    protected Stopwatch _timer;
    protected Coroutine _trialCoroutine;
    protected string _filePath;
    protected int _trialIndex;
    
    //The different steps in our test
    public enum steps { init, instructions, testing };
    protected steps _currentStep;
    
    //flag to define the time frame in which we accept answers
    protected bool _waitingForAnswer;
    
    // Start is called before the first frame update
    protected void Start()
    {
        _currentStep = steps.init;
        _timer = new Stopwatch();
    }
    
    protected void FinishTest()
    {
        InstructionsTextBehavior.instance.ShowInstructionText("Ok, the test is now finished! We will proceed with the next step now", 15);
    }
}
