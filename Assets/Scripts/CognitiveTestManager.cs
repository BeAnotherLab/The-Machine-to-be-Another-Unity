using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;

public class CognitiveTestManager : MonoBehaviour
{

    public static CognitiveTestManager instance;
    
     [SerializeField] Text _instructionsText;

     private enum steps
     {
         init,
         instructions,
         training,
         testing
     };

     private steps _currentStep;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _path = "Assets/task structure.json";
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(_path); 
        
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();

        _practiceTrials = new List<JSONObject>();
        _testTrials1 = new List<JSONObject>();
        _testTrials2 = new List<JSONObject>();
        _testTrials3 = new List<JSONObject>();
        _testTrials4 = new List<JSONObject>();
        
        for (int i = 0; i <= 26; i++) _practiceTrials.Add(_trials.list[i]);
        for (int i = 26; i < 78; i++) _testTrials1.Add(_trials.list[i]);
        for (int i = 78; i < 130; i++) _testTrials2.Add(_trials.list[i]);
        for (int i = 130; i < 182; i++) _testTrials3.Add(_trials.list[i]);
        for (int i = 182; i < 234; i++) _testTrials4.Add(_trials.list[i]);
        
_
        ListExtensions.Shuffle(_practiceTrials);
        ListExtensions.Shuffle(_testTrials1);
        ListExtensions.Shuffle(_testTrials2);
        ListExtensions.Shuffle(_testTrials3);
        ListExtensions.Shuffle(_testTrials4);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.instructions)
        {
            CognitiveTestInstructionsGUIBehavior.instance.Next();
        }
    }

    public void StartTest(string pronoun, string subjectID)
    {
        Debug.Log("starting test for " + pronoun + " with subjectID " + subjectID);
        CognitiveTestInstructionsGUIBehavior.instance.Init();
        _currentStep = steps.instructions;
        CognitiveTestSettingsGUI.instance.gameObject.SetActive(false);
    }

    public void StartTraining()
    {
        _currentStep = steps.training;
    }
}
