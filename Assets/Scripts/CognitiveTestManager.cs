using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitiveTestManager : MonoBehaviour
{

    public static CognitiveTestManager instance;
    
     [SerializeField] Text _instructionsText;


    private void Awake()
    {
        if (instance == null) instance = this;
    }

     // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartTest(string pronoun, string subjectID)
    {
        Debug.Log("starting test for " + pronoun + " with subjectID " + subjectID);
    }
}
