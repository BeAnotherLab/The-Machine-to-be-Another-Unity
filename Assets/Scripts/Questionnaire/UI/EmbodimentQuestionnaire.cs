using System;
using System.Collections;
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
    [SerializeField] private Button _nextButton;
    
    private int _currentQuestion;

    private void Start()
    {
        LoadFile("German");
    }

    public void SelectLanguage(string language)
    {
        LoadFile(language);
    }

    private void LoadFile(string language)
    {
        questionnaireInput.Clear();
        try
        {
            string line;
            StreamReader csvFileReader = new StreamReader("./Lists/questionnaire" + language + ".csv", Encoding.UTF8);
            using (csvFileReader)
            {
                line = csvFileReader.ReadLine();
                if (line != null)
                {
                    do
                    { // While there's lines left in the text file, do this:
                        string[] entries = line.Split('\t');
                        if (entries.Length > 0) questionnaireInput.Add(entries[0]);
                        line = csvFileReader.ReadLine();
                    }
                    while (line != null);
                }
                csvFileReader.Close(); // Done reading, close the reader and return true to broadcast success
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("{0}\n" + e.Message);
        }

        Initialize();
    }

    public void Initialize()
    {
        _responseSlider.value = 0.5f;
        _currentQuestion = 0;
        _questionText.text = questionnaireInput[_currentQuestion];
    }
    
    public void NextButton()
    {
        _currentQuestion++;
        if (_currentQuestion < questionnaireInput.Count)
        {
            _questionText.text = questionnaireInput[_currentQuestion];
        }
        else
        {
            _questionnaireNextEvent.Raise();
            _currentQuestion = 0;
        }

        _responseSlider.value = 0.5f;
    }
}
