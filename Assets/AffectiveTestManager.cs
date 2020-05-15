using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AffectiveTestManager : TestManager
{
    #region Private Fields

    //for parsing the trial structure JSONs
    private JSONObject _trials;
    private JSONObject _results;
    private JSONObject _finalTrialsList;

    private Dictionary<string, AudioClip> _audioClipsDictionary;        
    
    #endregion

    
    #region  Public Fields

    public static AffectiveTestManager instance;
    
    #endregion
    
    
    #region Monobehavior Methods

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        //Read the task structure from JSON
        StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/AffectiveTaskStructure.json"); 
        _trials = new JSONObject(reader.ReadToEnd());
        reader.Close();

        _finalTrialsList = new JSONObject();
        
        _audioClipsDictionary = new Dictionary<string, AudioClip>();
        AudioClip[] clips = Resources.LoadAll<AudioClip>("AffectiveTaskAudioClips");
        foreach (AudioClip clip in clips) _audioClipsDictionary.Add(clip.name + ".jpg", clip);
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space) && _currentStep == steps.instructions)
        {
            AffectiveTestInstructionsGUI.instance.Next();
        }
    }

    #endregion

    
    #region Public Methods
    
    public void StartTest(ExperimentStep experimentStep)
    {
        _trialCoroutine = StartCoroutine(ShowTrialCoroutine());
    }

    public void StartInstructions(string subjectID, string prepost)
    {
        _prepost = prepost;
        _subjectID = subjectID;
        var files = Directory.GetFiles(Application.dataPath);
        
        string filepath = Application.dataPath + "/" + "AffectiveTest" + subjectID + "_log.json";
        
        if (!File.Exists(filepath))
        {
            Debug.Log(" creating new file : " + filepath);
            _subjectID = subjectID;
            _filePath = filepath; 
            AffectiveTestInstructionsGUI.instance.Init();
            AffectiveTestSettingsGUI.instance.gameObject.SetActive(false); //hide settings GUI
            _currentStep = steps.instructions;       
        }
        else AffectiveTestSettingsGUI.instance.ShowExistingSubjectIDError();
    }
    
    #endregion
    
    
    #region Private Methods

    private IEnumerator ShowTrialCoroutine()
    {
        Debug.Log("trial index : " + _trialIndex);
        InstructionsTextBehavior.instance.ShowInstructionText(true, "+");
        
        yield return new WaitForSeconds(1);
        
        AffectiveTestInstructionsGUI.instance.ShowStimulus(_trials[_trialIndex]); //show stimulus
        if (_trials[_trialIndex].GetField("perspective").str == "other") //play audio
            AudioSource.PlayClipAtPoint(_audioClipsDictionary[_trials[_trialIndex].GetField("otherImage").str], transform.position);
        else
            AudioSource.PlayClipAtPoint(_audioClipsDictionary[_trials[_trialIndex].GetField("selfImage").str], transform.position);
        
        yield return new WaitForSeconds(4);
        
        AffectiveTestInstructionsGUI.instance.ShowRatingScale(); //show rating scale    
        
        yield return new WaitForSeconds(6);
        
        //time out!
    }
    
    private void WriteTestResults(string answer, double time)
    {
        _finalTrialsList[_trialIndex].AddField("answer", answer);
        _finalTrialsList[_trialIndex].AddField("prepost", _prepost);

        File.WriteAllText(_filePath, _finalTrialsList.Print());
        _trialIndex++;
    }

    
    #endregion
}
