using System;
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
            if (Input.GetMouseButtonDown(0)) GetAnswer(answer.yes);
            else if (Input.GetMouseButtonDown(1)) GetAnswer(answer.no);
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
        _currentStep = steps.testing;
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
        InstructionsTextBehavior.instance.ShowInstructionText(true, "Out of time!");
        _timer.Stop();
        
        yield return new WaitForSeconds(2);

        GetAnswer(answer.none);
    }

    private void GetAnswer(answer givenAnswer)
    {
        _waitingForAnswer = false;
        _givenAnswer = givenAnswer;
        _timer.Stop();        

        Debug.Log("time elapsed "  + _timer.ElapsedMilliseconds);
        StopCoroutine(_trialCoroutine);
        
        WriteTestResults(givenAnswer);
    }

    private void WriteTestResults(answer givenAnswer)
    {
        _finalTrialsList[_trialIndex].AddField("answer", givenAnswer.ToString());
        _finalTrialsList[_trialIndex].AddField("time", _timer.Elapsed.Milliseconds.ToString());
        _finalTrialsList[_trialIndex].AddField("prepost", _experimentData.experimentState.ToString());

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        _trialIndex++;
        _timer.Reset();
        
        if (_trialIndex == _finalTrialsList.Count)
        {
            FinishTest();
            VideoFeed.instance.CancelTweens();
            _experimentData.LoadNextScene();
        }
        else _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
        
        Debug.Log("Index is " + _trialIndex);
    }
    
    #endregion
    
}

