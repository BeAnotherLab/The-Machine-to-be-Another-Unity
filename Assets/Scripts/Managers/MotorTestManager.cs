﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = System.Random;

//'Trial_nr','Condition','Congruency','Finger','Resp','T_trial','T_cue','T_resp','RT');


public class MotorTestManager : TestManager
{

    #region Public Fields

    public enum Condition {congruentIndex, incongruentIndex, congruentMiddle, incongruentMiddle, baseIndex, baseMiddle};

    public delegate void MyTimerStart();
    public MyTimerStart OnTimerStart;
    
    #endregion
    

    #region Private fields

    [SerializeField] private int _numberTrials; 
    
    //the answers given by the subject
    private enum answer { indexFinger, middleFinger, none };
    private answer _givenAnswer;

    private bool _bothFingersOn;

    private List<Condition> _stimuli;

    private JSONObject _results;
    
    #endregion
    
    
    #region Public Fields

    public static MotorTestManager instance;
    
    #endregion


    #region Monobehavior methods
    
    private void Awake()
    {
        if (instance == null) instance = this;
        OnTimerStart += TimerStart;
    }

    private void TimerStart()
    {
        _timer.Start();   
        _waitingForAnswer = true;
        Debug.Log("timer start");
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

        _stimuli = new List<Condition>();
        
        foreach(Condition[] row in conditions)
            foreach(Condition condition in row)
                _stimuli.Add(condition);

        _results = new JSONObject();
    }

    private void Update()    
    {
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.instructions) //use space bar to go past instructions
        {
            MotorTestInstructionsGUIBehavior.instance.Next();
            _currentStep = steps.testing;
        }
        else if (_waitingForAnswer && _givenAnswer == answer.none) //get answer
        {
            if (Input.GetKeyUp(KeyCode.LeftArrow)) GetButtonUp(0);
            else if (Input.GetKeyUp(KeyCode.RightArrow)) GetButtonUp(1);
        }
        //if we just just pressed both busttons
        else if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightArrow) && !_bothFingersOn && _currentStep == steps.testing) 
        {
            _bothFingersOn = true;
            _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
        }
        //if we just lifted one finger during testing
        else if ((!Input.GetKey(KeyCode.LeftArrow) || !Input.GetKey(KeyCode.RightArrow)) && _bothFingersOn && _currentStep == steps.testing) 
        {
            StopCoroutine(_trialCoroutine);
            if(!_waitingForAnswer) MotorTestInstructionsGUIBehavior.instance.Stop();
            _bothFingersOn = false;
            StartTest();
        }
    }

    #endregion
    
    
    #region Public Methods
    
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
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Now press both buttons");
    }

    #endregion

    
    #region private methods
    
    private IEnumerator ShowTrialCoroutine()
    {
        Debug.Log("trial index : " + _trialIndex);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        MotorTestInstructionsGUIBehavior.instance.Stop();
        _givenAnswer = answer.none;

        yield return new WaitForSeconds(0.7f + 2f * UnityEngine.Random.value); // wait for a a random time

        MotorTestInstructionsGUIBehavior.instance.Play(_stimuli[_trialIndex]);
    }

    private void WriteTestResults(Condition condition, answer theAnswer, double time)
    {
        var stimulusResult = new JSONObject();
        stimulusResult.AddField("condition", Enum.GetName(typeof(Condition), _stimuli[_trialIndex]));
        stimulusResult.AddField("answer", Enum.GetName(typeof(answer), theAnswer));
        stimulusResult.AddField("time", time.ToString());
        stimulusResult.AddField("prepost", _prepost);
        
        _results.Add(stimulusResult);
        
        File.WriteAllText(_filePath, _results.Print());
        _trialIndex++;
        _timer.Reset();

        if (_trialIndex == _stimuli.Count)
        {
            FinishTest();
            MotorTestInstructionsGUIBehavior.instance.Stop();
        }
    }

    private void GetButtonUp(int button)
    {
        _waitingForAnswer = false;
        _timer.Stop();
        Debug.Log("time elapsed "  + _timer.ElapsedMilliseconds);
        if (_trialCoroutine != null) StopCoroutine(_trialCoroutine);
        
        if (button == 0)
        {
            _givenAnswer = answer.indexFinger;
            WriteTestResults(_stimuli[_trialIndex], _givenAnswer, _timer.Elapsed.Milliseconds);
        }
        else if (button == 1)
        {
            _givenAnswer = answer.middleFinger;
            WriteTestResults(_stimuli[_trialIndex], _givenAnswer, _timer.Elapsed.Milliseconds);
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
    
    #endregion
}