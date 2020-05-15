using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AffectiveTestInstructionsGUI : MonoBehaviour
{
    public static AffectiveTestInstructionsGUI instance;
    
    [SerializeField] private Image selfFrame;
    [SerializeField] private Image selfImage;
    [SerializeField] private Image otherFrame;
    [SerializeField] private Image otherImage;

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
        foreach (Sprite sprite in sprites) _spritesDictionary.Add(sprite.name, sprite);
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
        if (stimulus.GetField("perspective").str == "self")
        {
            selfFrame.color = Color.green;
            otherFrame.color = Color.red;
        }
        else
        {
            otherFrame.color = Color.green;
            selfFrame.color = Color.red;
        }

        selfImage.sprite = _spritesDictionary[stimulus.GetField("selfImage").str]; 
        otherImage.sprite = _spritesDictionary[stimulus.GetField("otherImage").str]; 
    }

    public void ShowRatingScale()
    {
        
    }
}
