using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Diagnostics;

public class CognitiveTestManager : MonoBehaviour
{
    #region Private Fields

    //for parsing the trial structure JSONs
    private int _trialIndex;
    private JSONObject _finalTrialsList;
    private JSONObject _trials;
    
    //the answers given by the subject
    private enum answer { yes, no, none };
    private answer _givenAnswer;

    //flag to defint the time frame in which we accept answers
    private bool _waitingForAnswer;

    //the trial instructions text canvas element
    [SerializeField] private Text _trialInstructionText;

    //The different steps in our test
    private enum steps { init, instructions, testing };
    private steps _currentStep;

    //the timer to measure reaction time
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
        
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader("Assets/task structure.json"); 
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();
        _finalTrialsList = new JSONObject();
        PrepareBlock(0, 26);
        PrepareBlock(26, 78);
        PrepareBlock(78, 130);
        PrepareBlock(130, 182);
        PrepareBlock(182, 234);
        
        _currentStep = steps.init;
        
        timer = new Stopwatch();
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

