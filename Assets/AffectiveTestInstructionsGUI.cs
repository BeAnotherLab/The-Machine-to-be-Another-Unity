using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AffectiveTestInstructionsGUI : MonoBehaviour
{
    public static AffectiveTestInstructionsGUI instance;
    
    [SerializeField] private Image _selfFrame;
    [SerializeField] private Image _selfImage;
    [SerializeField] private Image _otherFrame;
    [SerializeField] private Image _otherImage;
    [SerializeField] private CanvasGroup _affectiveInstructionsCanvasGroup;
    
    public Slider ratingScaleSlider;
    [SerializeField] private CanvasGroup _ratingScaleCanvasGroup;
    
    private Dictionary<string, Sprite> _spritesDictionary;
    
    [SerializeField] private GameObject[] _slides;
    private int _slideIndex;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>("AffectiveTaskImages");
        _spritesDictionary = new Dictionary<string, Sprite>();
        foreach (Sprite sprite in sprites) _spritesDictionary.Add(sprite.name + ".jpg", sprite);
    }

    public void Init()
    {
        _slides[0].SetActive(true);
    }
    
    public void Next()
    {
        _slides[_slideIndex].SetActive(false);

        if (_slideIndex < _slides.Length - 1 )
        {
            _slideIndex++;
            _slides[_slideIndex].SetActive(true);   
        }
        else
        {
            AffectiveTestManager.instance.StartTest(ExperimentStep.pre);
        }
    }
    
    public void ShowStimulus(JSONObject stimulus)
    {
        _affectiveInstructionsCanvasGroup.alpha = 1;
        _ratingScaleCanvasGroup.alpha = 0;
        
        if (stimulus.GetField("perspective").str == "self")
        {
            _selfFrame.color = Color.green;
            _otherFrame.color = Color.red;
        }
        else
        {
            _otherFrame.color = Color.green;
            _selfFrame.color = Color.red;
        }

        _selfImage.sprite = _spritesDictionary[stimulus.GetField("selfImage").str]; 
        _otherImage.sprite = _spritesDictionary[stimulus.GetField("otherImage").str]; 
    }

    public void ShowRatingScale()
    {
        ratingScaleSlider.value = 0;
        _affectiveInstructionsCanvasGroup.alpha = 0;
        _ratingScaleCanvasGroup.alpha = 1;
    }
}
