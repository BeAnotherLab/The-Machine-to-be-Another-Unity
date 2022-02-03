using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;

public class QuestionnaireUI : MonoBehaviour
{
    [SerializeField] private Transform _preRoot;
    [SerializeField] private Transform _postRoot;
    [SerializeField] private GameObject _videoConsent;
    
    [SerializeField] private GameEvent _preQuestionnaireFinished;
    [SerializeField] private BoolGameEvent _postQuestionnaireFinished; //bool param specfies if ended by finishing by removing headset

    [SerializeField] private QuestionnaireStateVariable _questionnaireState;
    [SerializeField] private UserStateVariable _previousSelfState;
    
    private List<GameObject> _preSlides;
    private List<GameObject> _postSlides;

    private int _slideIndex;

    private void Awake()
    {
        _preSlides = new List<GameObject>();
        _postSlides = new List<GameObject>();
        foreach (Transform preSlide in _preRoot) _preSlides.Add(preSlide.gameObject); 
        foreach (Transform postSlide in _postRoot) _postSlides.Add(postSlide.gameObject); 
    }

    private void Start()
    {
        _videoConsent.GetComponent<PanelDimmer>().Hide();
        
        foreach(Transform child in _preRoot){
            child.GetComponent<PanelDimmer>().Hide();
        }
        foreach(Transform child in _postRoot){
            child.GetComponent<PanelDimmer>().Hide();
        }
    }

    public void ExperienceFinished(bool withQuestionnaire) 
    {
        if (withQuestionnaire) //if we should show the questionnaire
        {
            _postSlides[0].GetComponent<PanelDimmer>().Show();
        }
    }

    public void OnStandby()
    {
        Hide();
        _questionnaireState.Value = QuestionnaireState.init;
    }
    
    public void ReadyToShowQuestionnaire(bool ready)
    {
    }

    public void VideoConsentGiven(bool consent)
    {
        _questionnaireState.Value = QuestionnaireState.pre;
        _preSlides[0].GetComponent<PanelDimmer>().Show();
    }

    public void NextButton()
    {
        if (_questionnaireState == QuestionnaireState.pre && _slideIndex == _preSlides.Count - 1)
        {
            _preQuestionnaireFinished.Raise();
            _preSlides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
            _slideIndex = 0;
            _questionnaireState.Value = QuestionnaireState.post;
            return;
        }
        if (_questionnaireState == QuestionnaireState.post && _slideIndex == _postSlides.Count - 1)
        {
            _postQuestionnaireFinished.Raise(false);
            _postSlides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
            _slideIndex = 0;
            return;
        }   
        if (_questionnaireState == QuestionnaireState.pre)
        {
            _preSlides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
            if (_slideIndex + 1 < _preSlides.Count)
                _preSlides[_slideIndex + 1].GetComponent<PanelDimmer>().Show(true);
        }
        else if (_questionnaireState == QuestionnaireState.post)
        {
            _postSlides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
            if (_slideIndex + 1 < _postSlides.Count)
                _postSlides[_slideIndex + 1].GetComponent<PanelDimmer>().Show(true);
        }
        _slideIndex++;
    }

    public void SelfStateChanged(UserState newState) //if self removed headest during post, do as if it had finished
    {
        if (_questionnaireState == QuestionnaireState.post)
        {
            if (_previousSelfState == UserState.readyToStart && newState == UserState.headsetOff) //if user removed headset
            {
                _postQuestionnaireFinished.Raise(true);
                Hide();
            }
        }
    }
    
    private void Hide()
    {
        if (_questionnaireState.Value == QuestionnaireState.pre)
        {
            _preSlides[_slideIndex].GetComponent<PanelDimmer>().Hide();
        }
        else if (_questionnaireState.Value == QuestionnaireState.post)
        {
            _postSlides[_slideIndex].GetComponent<PanelDimmer>().Hide();
        }
    }
}




