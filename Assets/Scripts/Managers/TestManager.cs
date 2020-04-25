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
    
    //the trial instructions text canvas element
    public Text _trialInstructionText;
    
    //flag to define the time frame in which we accept answers
    protected bool _waitingForAnswer;

    protected void ShowInstructionText(bool show, string text = "")
    {
        _trialInstructionText.transform.parent.gameObject.SetActive(show); //Show instructions canvas
        _trialInstructionText.text = text; //give feedback
    }
    
    // Start is called before the first frame update
    protected void Start()
    {
        _currentStep = steps.init;
        _timer = new Stopwatch();
        _finalTrialsList = new JSONObject();
    }
    
    protected IEnumerator FinishTest()
    {
        ShowInstructionText(true, "Ok, the test is now finished! We will proceed with the next step now");
        yield return new WaitForSeconds(3);
        ShowInstructionText(false);
        _trialIndex = 0;
    }

    protected void OutOfTime()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
