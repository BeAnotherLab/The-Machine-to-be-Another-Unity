using System;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class CustomQuestionnaireUI : MonoBehaviour
{
    [SerializeField] private GameEvent _questionnaireFinished;
    //[SerializeField] private ExperimentData _experimentData;
    
    private int _slideIndex;
    private bool _showing;
    
    private List<GameObject> _slides;

    private void Awake()
    {
        _slides = new List<GameObject>();
        foreach (Transform slide in transform) 
        {
            _slides.Add(slide.gameObject);
            slide.GetComponent<PanelDimmer>().Hide();
        }
    }

    private void Start()
    {
        Hide();
    }

    public void NextButton()
    {   
        if (_slideIndex == _slides.Count)
        {   
            Hide();
            _questionnaireFinished.Raise();
        }
        else
        {
            _slides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
            if (_slideIndex + 1 < _slides.Count)
                _slides[_slideIndex + 1].GetComponent<PanelDimmer>().Show(true);
        }
        
        _slideIndex++;
    }

    public void ShowQuestionnaire(bool show)
    {
        if (show)
        {
            _slides[0].GetComponent<PanelDimmer>().Show();
            _showing = true;
        } else 
        {
            Hide();
        }
    }
    
    private void Hide() 
    {
        _slides[_slideIndex].GetComponent<PanelDimmer>().Hide();
        _showing = false;
        _slideIndex = 0;
    }
}




