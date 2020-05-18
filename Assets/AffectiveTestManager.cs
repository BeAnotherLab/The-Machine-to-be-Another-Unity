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

    private bool _moveSlider;
    
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

        //TODO replace input with mouse
        if (Input.GetKeyUp(KeyCode.UpArrow) && _moveSlider)
            AffectiveTestInstructionsGUI.instance.ratingScaleSlider.value += 0.5f;
        else if (Input.GetKeyUp(KeyCode.DownArrow) && _moveSlider)
            AffectiveTestInstructionsGUI.instance.ratingScaleSlider.value -= 0.5f;
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

        _moveSlider = true;
        AffectiveTestInstructionsGUI.instance.ShowRatingScale(); //show rating scale    
        
        yield return new WaitForSeconds(6);

        _moveSlider = false;
        _trials[_trialIndex].AddField("answer", AffectiveTestInstructionsGUI.instance.ratingScaleSlider.value);
        _trials[_trialIndex].AddField("prepost", _prepost);

        File.WriteAllText(_filePath, _trials.Print());
        _trialIndex++;
        
        if (_trialIndex < _trials.Count) StartCoroutine(ShowTrialCoroutine());
        else FinishTest();
    }

    #endregion
}
