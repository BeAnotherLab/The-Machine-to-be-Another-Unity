using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorTestInstructionsGUIBehavior : MonoBehaviour
{
    public static MotorTestInstructionsGUIBehavior instance;

    [SerializeField] private float frameTime;
    [SerializeField] private Sprite _baseFrame, _baseIndexFrame, _baseMiddleFrame;
    [SerializeField] private Sprite[] _congruentIndexFrames, _congruentMiddleFingerFrames, _incongruentIndexFrames,  _incongruentMiddleFingerFrames;
    
    [SerializeField] private Image _frameImage;
    [SerializeField] private GameObject _frameGO;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
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
        _frameGO.SetActive(show);
    }

    public void Play(MotorTestManager.Condition condition)
    {
        Debug.Log("play condition " + condition);
        if(condition == MotorTestManager.Condition.congruentIndex) StartCoroutine(PlayVideo(_congruentIndexFrames, _baseIndexFrame));
        else if(condition == MotorTestManager.Condition.congruentMiddle) StartCoroutine(PlayVideo(_congruentMiddleFingerFrames, _baseMiddleFrame));
        else if(condition == MotorTestManager.Condition.incongruentIndex) StartCoroutine(PlayVideo(_incongruentIndexFrames, _baseIndexFrame));
        else if(condition == MotorTestManager.Condition.incongruentMiddle) StartCoroutine(PlayVideo(_incongruentMiddleFingerFrames, _baseMiddleFrame));
    }
    
    private IEnumerator PlayVideo(Sprite[] video, Sprite baseFingerFrame)
    {
        _frameImage.sprite = _baseFrame;
        yield return new WaitForSeconds(1.232f);
        _frameImage.sprite = baseFingerFrame;
        yield return new WaitForSeconds(1.5f);
        foreach (Sprite frame in video)
        {
            _frameImage.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }
    }
    
}
