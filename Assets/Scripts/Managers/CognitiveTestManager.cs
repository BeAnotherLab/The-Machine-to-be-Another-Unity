﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

public class CognitiveTestManager : MonoBehaviour
{
    
    #region Private Fields

    //params from settings GUI
    private string _pronoun;
    private string _subjectID;
    private string _subjectDirection;
    private string _prepost;
    
    //for parsing the trial structure JSONs
    private int _trialIndex;
    private JSONObject _finalTrialsList;
    private JSONObject _trials;
    private JSONObject _results;
    
    //the answers given by the subject
    private enum answer { yes, no, none };
    private answer _givenAnswer;

    //flag to define the time frame in which we accept answers
    private bool _waitingForAnswer;

    //the trial instructions text canvas element
    [SerializeField] private Text _trialInstructionText;

    //The different steps in our test
    public enum steps { init, instructions, practice, testing };
    private steps _currentStep;

    //the timer to measure reaction time
    private Stopwatch _timer;

    private Coroutine _trialCoroutine;

    private string _filePath;
    
    [SerializeField] string[] _blockNames;
    
    #endregion

    
    #region  Public Fields

    public static CognitiveTestManager instance;

    public delegate void PreTestsFinished();
    public event PreTestsFinished OnPreTestsFinished;
    
    #endregion


    #region Monobehavior Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        VideoFeed.instance.twoWayWap = true;
        
        //Read the task structure from JSON
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/task structure.json"); 
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();
        _finalTrialsList = new JSONObject();

        foreach (string blockName in _blockNames) PrepareBlock(blockName);
        
        _currentStep = steps.init;
        
        _timer = new Stopwatch();
    }
    
    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.init)
        {
            //TODO complement start button click
        }
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.instructions)
        {
            CognitiveTestInstructionsGUIBehavior.instance.Next();
        }
        else if (_waitingForAnswer && _givenAnswer == answer.none)
        {
            if (Input.GetMouseButtonDown(0)) GetClick(0);
            else if (Input.GetMouseButtonDown(1)) GetClick(1);
        }
    }    

    #endregion


    #region Public Methods

    public void StartInstructions(string pronoun, string subjectID, string subjectDirection, string prepost)
    {
        _pronoun = pronoun;
        _subjectDirection = subjectDirection;
        _prepost = prepost;
        var files = Directory.GetFiles(Application.dataPath);

        string filepath = Application.dataPath + "/" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            CognitiveTestInstructionsGUIBehavior.instance.Init();
            CognitiveSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
            VideoFeed.instance.SetDimmed(true);
        }
        else CognitiveSettingsGUI.instance.ShowExistingSubjectIDError();
    }
    
    public void StartTest(ExperimentStep experimentStep)
    {
        if (experimentStep == ExperimentStep.post) //if we are testing post intervention, go straight to testing
        {
            _currentStep = steps.testing;
            //if we skip practice, start at the index of the first block 
            _trialIndex = _trials.list.Where(trial => trial.GetField("field8").str == _blockNames[0]).ToList().Count;
        } else if (experimentStep == ExperimentStep.pre)
        {
            _currentStep = steps.practice;
        } //if we are testing pre intervention, show instructions and do a practice round
        
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

    
    private IEnumerator ShowTrialCoroutine(bool firstTest = false)
    {
        if (firstTest)
        {
            ShowInstructionText(true, "Ok, the trial is now finished! We will start the proper testing");
            yield return new WaitForSeconds(3);
            ShowInstructionText(false);
        }

        //initialize trial answer values
        _givenAnswer = answer.none;
        ShowInstructionText(true, "+");
        VideoFeed.instance.SetDimmed(true); //hide video feed
        RedDotsController.instance.Show("S0_O0_FR_EN"); //hide the dots
    
        yield return new WaitForSeconds(2);

        //Make sure to use the right pronoun
        string stim1 = _finalTrialsList[_trialIndex].GetField("stim1").str;
        if (stim1.Contains("SHE")) stim1 = _pronoun + " " + stim1[3];
        else stim1 = "You : " + stim1[3]; 
        ShowInstructionText(true, stim1); //show pronoun + number of balls
    
        yield return new WaitForSeconds(1.5f);
        
        Debug.Log("block : " + _finalTrialsList[_trialIndex].GetField("field8").str);
        Debug.Log("stim1 : " + _finalTrialsList[_trialIndex].GetField("stim1").str);
        Debug.Log("stim2 : " + _finalTrialsList[_trialIndex].GetField("stim2").str);

        _timer.Start();
        ShowInstructionText(false);
        VideoFeed.instance.SetDimmed(false); //display video feed
        MatchDirection(_finalTrialsList[_trialIndex].GetField("stim2").str[7]); //Make sure 
        RedDotsController.instance.Show(_finalTrialsList[_trialIndex].GetField("stim2").str); //show dots as indicated in file
        _waitingForAnswer = true;

        yield return new WaitForSeconds(4);

        _waitingForAnswer = false;
        WriteTestResults("none", _timer.ElapsedMilliseconds);
        ShowInstructionText(true, "Out of time!");
        _timer.Stop();
        _timer.Reset();
    
        yield return new WaitForSeconds(3);
    
        if (_trialIndex == _finalTrialsList.Count) StartCoroutine(FinishTest());
        else _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    private IEnumerator ShowFeedbackCoroutine(bool practiceFinished = false)
    {
        if (_givenAnswer != answer.none)
        {
            //write reaction time
            Debug.Log("correct answer : " + _finalTrialsList[_trialIndex].GetField("key").str);
            Debug.Log("given answer : " + _givenAnswer);
            
            //add answer
            if(    _finalTrialsList[_trialIndex].GetField("key").str == "c" && _givenAnswer == answer.yes 
                   || _finalTrialsList[_trialIndex].GetField("key").str == "n" && _givenAnswer == answer.no)
                ShowInstructionText(true, "Correct answer!"); 
            else 
                ShowInstructionText(true, "Wrong answer!");
        }
        
        yield return new WaitForSeconds(4);

        _trialCoroutine = StartCoroutine(ShowTrialCoroutine(practiceFinished));
    }
    
    private void ShowInstructionText(bool show, string text = "")
    {
        _trialInstructionText.transform.parent.gameObject.SetActive(show); //Show instructions canvas
        _trialInstructionText.text = text; //give feedback
    }

    private void GetClick(int button)
    {
        _waitingForAnswer = false;
        _timer.Stop();
        Debug.Log("time elapsed "  + _timer.ElapsedMilliseconds);
        StopCoroutine(_trialCoroutine);
        
        if (button == 0)
        {
            WriteTestResults("yes", _timer.Elapsed.Milliseconds);
            _givenAnswer = answer.yes;
        }
        else if (button == 1)
        {
            WriteTestResults("no", _timer.Elapsed.Milliseconds);
            _givenAnswer = answer.no;
        }
        
        if (_trialIndex == _finalTrialsList.Count) StartCoroutine(FinishTest());
        else if (_finalTrialsList[_trialIndex].GetField("type").str == "practice") StartCoroutine(ShowFeedbackCoroutine());
        else if (_finalTrialsList[_trialIndex].GetField("type").str == "test")
        {
            if (_currentStep == steps.practice) //if we just went from practice to proper testing
            {
                _trialCoroutine = StartCoroutine(ShowFeedbackCoroutine(true));
                _currentStep = steps.testing;
            } else if (_currentStep == steps.testing)
            {
                _trialCoroutine = StartCoroutine(ShowTrialCoroutine());    
            }
        }
    }

    private void WriteTestResults(string answer, double time)
    {
        _finalTrialsList[_trialIndex].AddField("answer", answer);
        _finalTrialsList[_trialIndex].AddField("time", time.ToString());
        _finalTrialsList[_trialIndex].AddField("prespost", _prepost);

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        _trialIndex++;
        _timer.Reset();
    }

    private IEnumerator FinishTest()
    {
        ShowInstructionText(true, "Ok, the test is now finished! We will proceed with the next step now");
        yield return new WaitForSeconds(3);
        OnPreTestsFinished();
        ShowInstructionText(false);
        _trialIndex = 0;
    }
    
    private void MatchDirection(char desiredDirection)
    {
        if (desiredDirection == 'R' && _subjectDirection == "Left")
        {
            VideoFeed.instance.FlipHorizontal();
            _subjectDirection = "Right";
        } else if (desiredDirection == 'L' && _subjectDirection == "Right")
        {
            VideoFeed.instance.FlipHorizontal();
            _subjectDirection = "Left";
        }
    }
    
    #endregion
    
    
}

