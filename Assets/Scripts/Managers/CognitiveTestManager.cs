﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class CognitiveTestManager : TestManager
{
    
    #region Private Fields

    //for parsing the trial structure JSONs
    private JSONObject _trials;
    private JSONObject _results;
    private JSONObject _finalTrialsList;
    
    //the answers given by the subject
    private enum answer { yes, no, none };
    private answer _givenAnswer;
    
    [SerializeField] string[] _blockNames;

    [SerializeField] private ExperimentData _experimentData;

    #endregion

    
    #region  Public Fields

    public static CognitiveTestManager instance;
    
    #endregion


    #region Monobehavior Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        base.Start();
        
        VideoFeed.instance.twoWayWap = true;
        VideoFeed.instance.SetDirection(_experimentData.subjectDirection);
        
        //Read the task structure from JSON
        var appendDebug = _experimentData.debug ? "debug" : "";
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + appendDebug + "task structure.json"); 
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();

        _finalTrialsList = new JSONObject();
        foreach (string blockName in _blockNames) PrepareBlock(blockName);

        StartInstructions();  
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0) && _currentStep == steps.instructions)
        {
            CognitiveTestInstructionsGUIBehavior.instance.Next();
        }
        else if (_waitingForAnswer && _givenAnswer == answer.none)
        {
            if (Input.GetMouseButtonDown(0)) GetClick(answer.yes);
            else if (Input.GetMouseButtonDown(1)) GetClick(answer.no);
        }
    }    

    #endregion


    #region Public Methods

    public void StartInstructions()
    {
        var files = Directory.GetFiles(Application.dataPath);
        string filepath = Application.dataPath + "/" + "CognitiveTest" + "-" + _experimentData.subjectID + "-" + _experimentData.experimentState.ToString() + "_log.json";
        Debug.Log(" creating new file : " + filepath);
        _filePath = filepath; 
        CognitiveTestInstructionsGUIBehavior.instance.Init();
        _currentStep = steps.instructions;       
       // VideoFeed.instance.SetDimmed(true);
    }
    
    public void StartTest()
    {
        _currentStep = steps.practice;
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    #endregion

    
    #region Private Methods
    
    private void PrepareBlock(string blockName)
    {
        List<JSONObject> jsonObjects = _trials.list.Where(trial => trial.GetField("field8").str == blockName).ToList();
        ListExtensions.Shuffle(jsonObjects); //shuffle that list
        foreach (JSONObject jsonObject in jsonObjects) _finalTrialsList.Add(jsonObject); //add it to the final list
    }
    
    private IEnumerator ShowTrialCoroutine()
    {
        //initialize trial answer values
        _givenAnswer = answer.none;
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        VideoFeed.instance.SetDimmed(true); //hide video feed
        RedDotsController.instance.Show("S0_O0_FR_EN"); //hide the dots
    
        yield return new WaitForSeconds(2);

        //Make sure to use the right pronoun
        string stim1 = _finalTrialsList[_trialIndex].GetField("stim1").str;
        if (stim1.Contains("SHE")) stim1 = "" + "She : " + stim1[3];
        else stim1 = "You : " + stim1[3]; 
        InstructionsTextBehavior.instance.ShowInstructionText(true, stim1); //show pronoun + number of balls
    
        yield return new WaitForSeconds(1.5f);
        
        Debug.Log("block : " + _finalTrialsList[_trialIndex].GetField("field8").str);
        Debug.Log("stim1 : " + _finalTrialsList[_trialIndex].GetField("stim1").str);
        Debug.Log("stim2 : " + _finalTrialsList[_trialIndex].GetField("stim2").str);

        _timer.Start();
        InstructionsTextBehavior.instance.ShowInstructionText(false);
        VideoFeed.instance.SetDimmed(false); //display video feed
        VideoFeed.instance.MatchDirection(_finalTrialsList[_trialIndex].GetField("stim2").str[7]);  
        RedDotsController.instance.Show(_finalTrialsList[_trialIndex].GetField("stim2").str); //show dots as indicated in file
        _waitingForAnswer = true;

        yield return new WaitForSeconds(4);

        _waitingForAnswer = false;
        WriteTestResults(answer.none, _timer.ElapsedMilliseconds);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Out of time!");
        _timer.Stop();
        _timer.Reset();
    
        yield return new WaitForSeconds(3);

        if (_trialIndex == _finalTrialsList.Count)
        {
            FinishTest();
            VideoFeed.instance.CancelTweens();
            if (_experimentData.experimentState == ExperimentState.pre) SceneManager.LoadScene("SparkSwap");
        }
        else _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    private IEnumerator ShowFeedbackCoroutine(answer givenAnswer, bool practiceFinished = false)
    {
        if (givenAnswer != answer.none)
        {
            //write reaction time
            Debug.Log("correct answer : " + _finalTrialsList[_trialIndex].GetField("key").str);
            Debug.Log("given answer : " + givenAnswer);
            
            //add answer
            if(    _finalTrialsList[_trialIndex].GetField("key").str == "c" && givenAnswer == answer.yes 
                   || _finalTrialsList[_trialIndex].GetField("key").str == "n" && givenAnswer == answer.no)
                InstructionsTextBehavior.instance.ShowInstructionText(true, "Correct answer!"); 
            else 
                InstructionsTextBehavior.instance.ShowInstructionText(true, "Wrong answer!");
        }
        
        yield return new WaitForSeconds(4);

        if (practiceFinished) //if we just finished practicing
        {
            InstructionsTextBehavior.instance.ShowInstructionText("Ok, the trial is now finished! We will start the proper testing", 3);
            yield return new WaitForSeconds(3);
        }
        
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    private void GetClick(answer givenAnswer)
    {
        _waitingForAnswer = false;
        _givenAnswer = givenAnswer;
        _timer.Stop();        

        Debug.Log("time elapsed "  + _timer.ElapsedMilliseconds);
        StopCoroutine(_trialCoroutine);

        var practiceFinished = 
            _currentStep == steps.practice && 
            _finalTrialsList[_trialIndex + 1].GetField("type").str == "test";
            
        //if we're practicing, show feedback before continuing
        if (_currentStep == steps.practice) StartCoroutine(ShowFeedbackCoroutine(givenAnswer, practiceFinished));
        //else show go to next trial
        else _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    
        if (practiceFinished) _currentStep = steps.testing;
        WriteTestResults(givenAnswer, _timer.Elapsed.Milliseconds);
    }

    private void WriteTestResults(answer givenAnswer, double time)
    {
        _finalTrialsList[_trialIndex].AddField("answer", givenAnswer.ToString());
        _finalTrialsList[_trialIndex].AddField("time", time.ToString());
        _finalTrialsList[_trialIndex].AddField("prepost", _experimentData.experimentState.ToString());

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        _trialIndex++;
        _timer.Reset();
        Debug.Log("Index is " + _trialIndex);
    }
    
    #endregion
    
}

