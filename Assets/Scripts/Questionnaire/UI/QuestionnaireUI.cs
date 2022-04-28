using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;

public class QuestionnaireUI : MonoBehaviour
{
    [SerializeField] private Transform _preRoot;
    [SerializeField] private Transform _postRoot;
    [SerializeField] private GameObject _videoConsent;
    
    [SerializeField] private GameEvent _preQuestionnaireFinished;
    [SerializeField] private GameEvent _postQuestionnaireFinished;

    [SerializeField] private QuestionnaireStateVariable _questionnaireState;
    [SerializeField] private UserStateVariable _previousSelfState;
    [SerializeField] private UserStateVariable _previousOtherState;
    
    private List<GameObject> _preSlides;
    private List<GameObject> _postSlides;

    private int _slideIndex;
    private bool _showing;
    
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
            _questionnaireState.Value = QuestionnaireState.post;
            _postSlides[0].GetComponent<PanelDimmer>().Show();
            _showing = true;
        }
    }

    public void OnStandby()
    {
        Hide();
        _questionnaireState.Value = QuestionnaireState.init;
    }
    
    public void ReadyToShowQuestionnaire(bool ready)
    {
        if (ready)
        {
            _questionnaireState.Value = QuestionnaireState.pre;
            _preSlides[0].GetComponent<PanelDimmer>().Show();    
            _showing = true;
        }
    }

    public void NextButton()
    {
        //reached last pre questionnaire question 
        if (_questionnaireState == QuestionnaireState.pre && _slideIndex == _preSlides.Count - 1)
        {
            QuestionnaireFinished(_preQuestionnaireFinished);
            return;
        }
        //reached last post questionnaire question
        if (_questionnaireState == QuestionnaireState.post && _slideIndex == _postSlides.Count - 1)
        {
            QuestionnaireFinished(_postQuestionnaireFinished);
            return;
        }   
        //next pre questionnaire question
        if (_questionnaireState == QuestionnaireState.pre) NextSlide(_preSlides);
            
        //next post questionnaire question
        else if (_questionnaireState == QuestionnaireState.post) NextSlide(_postSlides);
        
        _slideIndex++;
    }

    public void SelfStateChanged(UserState newState) //if self removed headset during post, do as if it had finished
    {
        if (_questionnaireState == QuestionnaireState.post)
        {
            if (_previousSelfState == UserState.readyToStart && newState == UserState.headsetOff && _showing) //if user removed headset
            {
                _postQuestionnaireFinished.Raise();
                Hide();
            }
        }
    }
    
    public void OtherStateChanged(UserState newState) //Hide questionnaire if we're doing questionnaire pre and the other removed the headset.
    {
        if (_previousOtherState == UserState.readyToStart 
            && newState == UserState.headsetOff
            && _showing
            && _questionnaireState == QuestionnaireState.pre) //if user removed headset
        {
            Hide();
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
        _showing = false;
        _slideIndex = 0;
    }
    
    private void NextSlide(List<GameObject> slides)
    {
        slides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
        if (_slideIndex + 1 < slides.Count)
            slides[_slideIndex + 1].GetComponent<PanelDimmer>().Show(true);
    }
    
    private void QuestionnaireFinished(GameEvent questionnaireFinishedEvent)
    {
        questionnaireFinishedEvent.Raise();
        _postSlides[_slideIndex].GetComponent<PanelDimmer>().Show(false);
        _showing = false;
        _slideIndex = 0;
    }
}




