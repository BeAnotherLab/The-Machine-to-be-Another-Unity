﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net.Mime;
using System.Text;
using ScriptableObjectArchitecture;
using UnityEngine.UI;

public class EmbodimentQuestionnaire : MonoBehaviour
{
    public List<string> questionnaireInput  = new List<string>();

    [SerializeField] private Text _questionText;
    [SerializeField] private Scrollbar _responseSlider;
    [SerializeField] private GameEvent _questionnaireNextEvent;
    
    private int currentQuestion;
    
    private void Start() 
    {
        try { 
            string line;
            StreamReader csvFileReader = new StreamReader("./Lists/questionnaire.csv", Encoding.Default);
            using (csvFileReader) {
                line = csvFileReader.ReadLine();
                if (line != null) {
                    do { // While there's lines left in the text file, do this:
                        string[] entries = line.Split(',');
                        if (entries.Length > 0) questionnaireInput.Add (entries[0]);
                        line = csvFileReader.ReadLine();
                    }
                    while (line != null);
                } 
                csvFileReader.Close(); // Done reading, close the reader and return true to broadcast success
            }
        }
        catch (System.Exception e) {
            Debug.Log("{0}\n" + e.Message);
        }
    }

    public void Initialize()
    {
        _responseSlider.value = 0.5f;
        currentQuestion = 0;
        _questionText.text = questionnaireInput[currentQuestion];
    }
    
    public void NextButton()
    {
        if (currentQuestion < questionnaireInput.Count)
        {
            _questionText.text = questionnaireInput[currentQuestion];
        }
        else
        {
            _questionnaireNextEvent.Raise();
        }

        currentQuestion++;
        _responseSlider.value = 0.5f;
    }
}
