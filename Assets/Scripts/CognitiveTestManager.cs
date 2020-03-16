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
