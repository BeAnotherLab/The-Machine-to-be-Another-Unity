using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class MotorTestManager : MonoBehaviour
{
    
    public enum steps {init, instructions, testing};
    private steps _currentStep;
    
    private string _subjectID;
    private string _prepost;

    private int _trialIndex;

    //the timer to measure reaction time
    private Stopwatch _timer;
    private Coroutine _trialCoroutine;
    private string _filePath;

    private JSONObject _trials;
    
    //the trial instructions text canvas element
    [SerializeField] private Text _trialInstructionText;
    
    #region Public Fields

    public static MotorTestManager instance;
    
    #endregion
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _trials = new JSONObject();
    }

    public void StartInstructions(string subjectID, string prepost)
    {
        _prepost = prepost;
        _subjectID = subjectID;
        var files = Directory.GetFiles(Application.dataPath);
        
        string filepath = Application.dataPath + "/" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            MotorTestInstructionsGUIBehavior.instance.Init();
            MotorTestSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
            VideoFeed.instance.SetDimmed(true);
        }
        else MotorTestSettingsGUI.instance.ShowExistingSubjectIDError();
    }

    public void StartText()
    {
        
    }

    private IEnumerator ShowTrialCoroutine()
    {
        ShowInstructionText(true, "+");
        VideoFeed.instance.SetDimmed(true); //hide video feed
        
        yield return new WaitForSeconds(1);
        
    }
    
    private void ShowInstructionText(bool show, string text = "")
    {
        _trialInstructionText.transform.parent.gameObject.SetActive(show); //Show instructions canvas
        _trialInstructionText.text = text; //give feedback
    }

    private void WriteTestResults(string answer, double time)
    {
        _trials[_trialIndex].AddField("answer", answer);
        _trials[_trialIndex].AddField("time", time.ToString());
        _trials[_trialIndex].AddField("prepost", _prepost);

        File.WriteAllText(_filePath, _trials.Print());
        _trialIndex++;
        _timer.Reset();
    }

    
}
