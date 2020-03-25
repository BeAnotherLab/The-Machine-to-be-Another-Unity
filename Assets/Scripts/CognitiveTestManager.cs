using System;
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
    private enum steps { init, instructions, practice, testing };
    private steps _currentStep;

    //the timer to measure reaction time
    private Stopwatch _timer;

    private Coroutine _trialCoroutine;

    private string _filePath;
    
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
        VideoFeed.instance.twoWayWap = true;
        
        //Read the task structure from JSON
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/task structure.json"); 
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();
        _finalTrialsList = new JSONObject();
        PrepareBlock(0, 26); //practice block
        PrepareBlock(26, 78); //block 1
        PrepareBlock(78, 130); //block 2
        PrepareBlock(130, 182); //block 3
        PrepareBlock(182, 234); //block 4
        
        _currentStep = steps.init;
        
        _timer = new Stopwatch();
    }

    private void PrepareBlock(int startIndex, int endIndex)
    {
        List<JSONObject> jsonObjects = new List<JSONObject>(); //create list of JSONObjects
        for (int i = startIndex; i < endIndex; i++) jsonObjects.Add(_trials.list[i]); //add elements between our boundaries
        ListExtensions.Shuffle(jsonObjects); //shuffle that list
        foreach (JSONObject jsonObject in jsonObjects) _finalTrialsList.Add(jsonObject); //add it to the final list
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

    public void StartInstructions(string pronoun, string subjectID, string subjectDirection)
    {
        _pronoun = pronoun;
        _subjectDirection = subjectDirection;
        var files = Directory.GetFiles(Application.dataPath);

        string filepath = Application.dataPath + "/" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            CognitiveTestInstructionsGUIBehavior.instance.Init();
            CognitiveTestSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
            VideoFeed.instance.SetDimmed(true);
        }
        else CognitiveTestSettingsGUI.instance.ShowExistingSubjectIDError();
    }
    
    public void StartTest()
    {
        _currentStep = steps.practice;
        _trialIndex = 0;
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    #endregion

    
    #region Private Methods
    
    private IEnumerator ShowTrialCoroutine(bool firstTest = false)
    {
        if (firstTest)
        {
            ShowInstructionText(true, "Ok, the trial is now finished! We will start the proper testing");
            yield return new WaitForSeconds(3);
            ShowInstructionText(false);
        }
        
        //initialize trial answer values
        _trialIndex++;
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
        
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
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
        
        if (_finalTrialsList[_trialIndex].GetField("type").str == "practice") StartCoroutine(ShowFeedbackCoroutine());
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

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        
        _timer.Reset();
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

