using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CognitiveTestManager : MonoBehaviour
{
    private string _path;
    private string _pronoun;
    private int _trialIndex;

    private JSONObject _trials;
    private List<JSONObject> _practiceTrials;
    private List<JSONObject> _testTrials1, _testTrials2, _testTrials3, _testTrials4;
    private JSONObject _finalTrialsList;
    
    public static CognitiveTestManager instance;

    [SerializeField] Text _trialInstructionText;

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
        
        foreach (JSONObject jsonObject in _testTrials4) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials3) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials2) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _testTrials1) _finalTrialsList.Add(jsonObject);
        foreach (JSONObject jsonObject in _practiceTrials) _finalTrialsList.Add(jsonObject);
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
        _pronoun = pronoun;        
        CognitiveTestInstructionsGUIBehavior.instance.Init();
        _currentStep = steps.instructions;
        CognitiveTestSettingsGUI.instance.gameObject.SetActive(false);
        _trialIndex = 0;
        StartCoroutine(ShowTrialCoroutine());
    }

    public void StartTraining()
    {
        _currentStep = steps.training;
    }

    private IEnumerator ShowTrialCoroutine()
    {
        _trialInstructionText.text = "+";

        yield return new WaitForSeconds(2);

        _trialInstructionText.text = _finalTrialsList[_trialIndex].GetField("stim1").str;
        
        yield return new WaitForSeconds(2);
        
        VideoFeed.instance.SetDimmed(false);
        
        VideoFeed.instance.FlipHorizontal(); //if need to change direction

        RedDotsController.instance.Show(_finalTrialsList[_trialIndex].GetField("stim2").str);

        yield return new WaitForSeconds(4);
        
        _trialIndex++;

        StartCoroutine(ShowTrialCoroutine());
    }
    

}

