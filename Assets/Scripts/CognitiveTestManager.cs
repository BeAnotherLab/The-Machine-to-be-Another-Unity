using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

public class CognitiveTestManager : MonoBehaviour
{
    #region Private Fields

    private int _trialIndex;

    //intermediary JSON objects
   
    private JSONObject _finalTrialsList;

    private JSONObject _results; //the JSON object that gets written as a file in the end
        
    private enum answer { yes, no, none };

    private answer _givenAnswer;
    private bool _waitingForAnswer;

    [SerializeField] private Text _trialInstructionText;

    private enum steps { init, instructions, testing };

    private steps _currentStep;

    private Stopwatch timer;

    private Coroutine _trialCoroutine;
    
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
        
        JSONObject _trials;
        List<JSONObject> _practiceTrials;
        List<JSONObject> _testTrials1, _testTrials2, _testTrials3, _testTrials4;
        
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader("Assets/task structure.json"); 
        
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();

        //TODO shorten
        _practiceTrials = new List<JSONObject>();
        _testTrials1 = new List<JSONObject>();
        _testTrials2 = new List<JSONObject>();
        _testTrials3 = new List<JSONObject>();
        _testTrials4 = new List<JSONObject>();
        _finalTrialsList = new JSONObject();
        
        for (int i = 0; i <= 26; i++) _practiceTrials.Add(_trials.list[i]);
        for (int i = 26; i < 78; i++) _testTrials1.Add(_trials.list[i]);
        for (int i = 78; i < 130; i++) _testTrials2.Add(_trials.list[i]);
        for (int i = 130; i < 182; i++) _testTrials3.Add(_trials.list[i]);
        for (int i = 182; i < 234; i++) _testTrials4.Add(_trials.list[i]);
        
        ListExtensions.Shuffle(_practiceTrials);
        ListExtensions.Shuffle(_testTrials1);
        ListExtensions.Shuffle(_testTrials2);
        ListExtensions.Shuffle(_testTrials3);
        ListExtensions.Shuffle(_testTrials4);
        
        foreach (JSONObject jsonObject in _practiceTrials) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials1) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials2) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials3) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials4) _finalTrialsList.Add(jsonObject);

        _currentStep = steps.init;
        
        timer = new Stopwatch();
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

    public void StartInstructions(string pronoun, string subjectID)
    {
        //TODO save pronoun and subjectID in playerprefs
        //TODOCheck if SubjectID does not already exist
        CognitiveTestInstructionsGUIBehavior.instance.Init();
        CognitiveTestSettingsGUI.instance.gameObject.SetActive(false);
        _currentStep = steps.instructions;
    }
    
    public void StartTest()
    {
        _currentStep = steps.testing;
        _trialIndex = 0;
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    #endregion

    
    #region Private Methods
    
    private IEnumerator ShowTrialCoroutine()
    {
        _trialIndex++;
        
        //initialize trial answer values
        timer.Start();
        _givenAnswer = answer.none;
        
        ShowInstructionText(true, "+");
        VideoFeed.instance.SetDimmed(true); //hide video feed

        yield return new WaitForSeconds(2);

        //TODO load pronoun from settings
        ShowInstructionText(true, _finalTrialsList[_trialIndex].GetField("stim1").str); //show pronoun + number of balls
        
        yield return new WaitForSeconds(2);
        _waitingForAnswer = true;
        ShowInstructionText(false);
        VideoFeed.instance.SetDimmed(false); //display video feed
        //VideoFeed.instance.FlipHorizontal(); //TODO if need to change direction
        RedDotsController.instance.Show(_finalTrialsList[_trialIndex].GetField("stim2").str); //show dots as indicated in file
        //get expected answer
        
        yield return new WaitForSeconds(4);
        ShowInstructionText(true, "Out of time!");
        
        yield return new WaitForSeconds(3);
        
        _waitingForAnswer = false;
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    private IEnumerator ShowFeedbackCoroutine()
    {
        StopCoroutine(_trialCoroutine);
        if (_givenAnswer != answer.none)
        {
            //write reaction time
            UnityEngine.Debug.Log("correct answer : " + _finalTrialsList[_trialIndex].GetField("key").str);
            UnityEngine.Debug.Log("given answer : " + _givenAnswer);
            UnityEngine.Debug.Log("Response time : " + (timer.Elapsed.Milliseconds));
            
            //add answer
            if(    _finalTrialsList[_trialIndex].GetField("key").str == "c" && _givenAnswer == answer.yes 
                   || _finalTrialsList[_trialIndex].GetField("key").str == "n" && _givenAnswer == answer.no)
                ShowInstructionText(true, "True answer!"); //good answer
            else 
                ShowInstructionText(true, "Bad answer!"); //bad answer
        }
        
        yield return new WaitForSeconds(4);

        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }
    
    private void ShowInstructionText(bool show, string text = "")
    {
        _trialInstructionText.transform.parent.gameObject.SetActive(show); //Show instructions canvas
        _trialInstructionText.text = text; //give feedback
    }

    private void GetClick(int button)
    {
        if (button == 0) _givenAnswer = answer.yes;
        if (button == 1) _givenAnswer = answer.no;
        
        if (_finalTrialsList[_trialIndex].GetField("type").str == "practice") StartCoroutine(ShowFeedbackCoroutine());
        else if (_finalTrialsList[_trialIndex].GetField("type").str == "test") _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }    
    
    #endregion
    
}

