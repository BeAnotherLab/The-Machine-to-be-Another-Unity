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

    public void StartPostQuestionnaire(bool start)  
    {
        if (start) StartQuestionnaire(QuestionnaireState.post);  
    }
    
    public void StartPreQuestionnaire(bool start)
    {
        if (start) StartQuestionnaire(QuestionnaireState.pre);
    }

    public void EndPreQuestionnaire()
    {
        if (_showing) QuestionnaireFinished(_preQuestionnaireFinished);
    }

    public void EndPostQuestionnaire()
    {
        if (_showing) QuestionnaireFinished(_postQuestionnaireFinished);
    }
    
    public void Init() 
    {
        Hide();
        _questionnaireState.Value = QuestionnaireState.init;
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

    private void StartQuestionnaire(QuestionnaireState state)
    {
        _questionnaireState.Value = state;
        if(state == QuestionnaireState.pre) _preSlides[0].GetComponent<PanelDimmer>().Show();
        if(state == QuestionnaireState.post) _postSlides[0].GetComponent<PanelDimmer>().Show();
        _showing = true;
    }
    
    private void Hide()
    {
        if (_questionnaireState.Value == QuestionnaireState.pre) 
            _preSlides[_slideIndex].GetComponent<PanelDimmer>().Hide();
        else if (_questionnaireState.Value == QuestionnaireState.post) 
            _postSlides[_slideIndex].GetComponent<PanelDimmer>().Hide();
        
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
        Hide();
    }
}




