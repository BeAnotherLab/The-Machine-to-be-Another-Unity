using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorTestInstructionsGUIBehavior : MonoBehaviour
{
    public static MotorTestInstructionsGUIBehavior instance;

    [SerializeField] private int frameTime;
    [SerializeField] private Sprite _baseFrame, _baseIndexFrame, _baseMiddleFrame;
    [SerializeField] private Sprite[] _congruentIndexFrames, _congruentMiddleFingerFrames, _incongruentIndexFrames,  _incongruentMiddleFingerFrames;

    private Image _frameImage;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        _frameImage = GetComponent<Image>();
    }

    public void Init()
    {
        InstructionsTextBehavior.instance.ShowInstructionText(true, "");
    }

    public void Next()
    {
        InstructionsTextBehavior.instance.ShowInstructionText(false, "");
        MotorTestManager.instance.StartTest();
    }
    
    public void ShowAnimation(bool show)
    {
        GetComponent<CanvasGroup>().alpha = show ? 0 : 1;
    }

    public void Play(MotorTestManager.Condition condition)
    {
        if (condition == MotorTestManager.Condition.congruentIndex) StartCoroutine(PlayVideo(_congruentIndexFrames));
        if(condition == MotorTestManager.Condition.congruentMiddle) StartCoroutine(PlayVideo(_congruentMiddleFingerFrames));
        if(condition == MotorTestManager.Condition.incongruentIndex) StartCoroutine(PlayVideo(_incongruentIndexFrames));
        if(condition == MotorTestManager.Condition.incongruentMiddle) StartCoroutine(PlayVideo(_incongruentMiddleFingerFrames));
    }
    
    private IEnumerator PlayVideo(Sprite[] video)
    {
        _frameImage.sprite = _baseFrame;

        yield return new WaitForSeconds(frameTime);
        
        foreach (Sprite frame in video)
        {
            _frameImage.sprite = frame;
        }
    }
    
}
