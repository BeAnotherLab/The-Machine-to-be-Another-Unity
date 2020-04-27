using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
//'Trial_nr','Condition','Congruency','Finger','Resp','T_trial','T_cue','T_resp','RT');


public class MotorTestManager : TestManager
{

    public enum Condition {congruentIndex, incongruentIndex, congruentMiddle, incongruentMiddle, baseIndex, baseMiddle};
    public Condition _currentCondition;
    
    [SerializeField] private int _numberTrials; 
    
    //the answers given by the subject
    private enum answer { left, right, none };
    private answer _givenAnswer;

    private bool _bothFingersOn;
    
    #region Public Fields

    public static MotorTestManager instance;
    
    #endregion
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        base.Start();
        
        //first create an array with "_numberTrials" repetitions of each of the 6 stiumulus combinations and randomize their order
        Condition[][] conditions = new Condition [_numberTrials][];
        
        for (int i = 0; i < _numberTrials; i++)
        {
            int j = 0;
            conditions[i] = new Condition[6];
            foreach (Condition condition in Enum.GetValues(typeof(Condition)))
            {
                conditions[i][j] = condition;
                j++;
            }
            Reshuffle(conditions[i]);
        }
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.instructions)
        {
            MotorTestInstructionsGUIBehavior.instance.Next();
            _currentStep = steps.testing;
        }
        else if (_waitingForAnswer && _givenAnswer == answer.none) //get answer
        {
            if (Input.GetMouseButtonUp(0)) GetButtonUp(0);
            else if (Input.GetMouseButtonUp(1)) GetButtonUp(1);
        }
        else if (Input.GetMouseButtonUp(0) && Input.GetMouseButtonDown(1) && !_bothFingersOn && _currentStep == steps.testing)
        {
            _bothFingersOn = true;
            _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
        }
    }

    public void StartInstructions(string subjectID, string prepost)
    {
        _prepost = prepost;
        _subjectID = subjectID;
        var files = Directory.GetFiles(Application.dataPath);
        
        string filepath = Application.dataPath + "/" + "MotorTest" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            MotorTestInstructionsGUIBehavior.instance.Init();
            MotorTestSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
        }
        else MotorTestSettingsGUI.instance.ShowExistingSubjectIDError();
    }

    public void StartTest()
    {
        _currentStep = steps.practice;
        ShowInstructionText(true, "Now press both buttons and we'll start");
    }

    private IEnumerator ShowTrialCoroutine()
    {
        ShowInstructionText(true, "+");
        _givenAnswer = answer.none;
        MotorTestInstructionsGUIBehavior.instance.ShowAnimation(false);
        
        yield return new WaitForSeconds(1);

        _waitingForAnswer = true;
        MotorTestInstructionsGUIBehavior.instance.ShowAnimation(true); //show trial animation
        MotorTestInstructionsGUIBehavior.instance.Play(_currentCondition);
        _timer.Start();
        
        yield return new WaitForSeconds(3); //TODO check animation length

        //ran out of time
        _waitingForAnswer = false;
        WriteTestResults("none", _timer.ElapsedMilliseconds);
        ShowInstructionText(true, "Out of time!");
        _timer.Stop();
        _timer.Reset();
        
        yield return new WaitForSeconds(3);
    }

    private void WriteTestResults(string answer, double time)
    {
        _finalTrialsList[_trialIndex].AddField("answer", answer);
        _finalTrialsList[_trialIndex].AddField("time", time.ToString());
        _finalTrialsList[_trialIndex].AddField("prepost", _prepost);

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        _trialIndex++;
        _timer.Reset();
    }

    private void GetButtonUp(int button)
    {
        _waitingForAnswer = false;
        _timer.Stop();
        Debug.Log("time elapsed "  + _timer.ElapsedMilliseconds);
        StopCoroutine(_trialCoroutine);
        
        if (button == 0)
        {
            WriteTestResults("yes", _timer.Elapsed.Milliseconds);
            _givenAnswer = answer.left;
        }
        else if (button == 1)
        {
            WriteTestResults("no", _timer.Elapsed.Milliseconds);
            _givenAnswer = answer.right;
        }
        
        if (_trialIndex == _finalTrialsList.Count) StartCoroutine(FinishTest());
        else if (_finalTrialsList[_trialIndex].GetField("type").str == "test")
        {
           if (_currentStep == steps.testing)
            {
                _trialCoroutine = StartCoroutine(ShowTrialCoroutine());    
            }
        }
    }

    private void Reshuffle(Condition[] array)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < array.Length; t++ )
        {
            Condition tmp = array[t];
            int r = UnityEngine.Random.Range(t, array.Length);
            array[t] = array[r];
            array[r] = tmp;
        }
    }
    
}
