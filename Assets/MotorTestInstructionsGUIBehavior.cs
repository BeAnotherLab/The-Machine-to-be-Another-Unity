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

    private Coroutine _playbackCoroutine;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }
    
    public void Init()
    {
        var initText = @"Finger exercise task

        1 = Raise your index finger
        2 = Raise your middle finger

        You will see different hand movements shown on the screen. However, your task is to focus on the number that appears on the screen and to raise you index or middle finger as soon as possible, once a number has been displayed.

        Ready? Press the spacebar to start!";
        
        InstructionsTextBehavior.instance.ShowInstructionText(true, initText);
    }

    public void Next()
    {
        InstructionsTextBehavior.instance.ShowInstructionText(false, "");
        MotorTestManager.instance.StartTest();
    }
    
    public void Play(MotorTestManager.Condition condition)
    {
        Debug.Log("play condition " + condition);
        
        if(condition == MotorTestManager.Condition.congruentIndex) 
            _playbackCoroutine = StartCoroutine(PlayVideo(_congruentIndexFrames, _baseIndexFrame));
        else if(condition == MotorTestManager.Condition.congruentMiddle) 
            _playbackCoroutine = StartCoroutine(PlayVideo(_congruentMiddleFingerFrames, _baseMiddleFrame));
        else if(condition == MotorTestManager.Condition.incongruentIndex) 
            _playbackCoroutine = StartCoroutine(PlayVideo(_incongruentIndexFrames, _baseIndexFrame));
        else if(condition == MotorTestManager.Condition.incongruentMiddle) 
            _playbackCoroutine = StartCoroutine(PlayVideo(_incongruentMiddleFingerFrames, _baseMiddleFrame));
        else if(condition == MotorTestManager.Condition.baseIndex) 
            _playbackCoroutine = StartCoroutine(PlayVideo(new Sprite[] {_baseIndexFrame, _baseIndexFrame, _baseIndexFrame}, _baseIndexFrame, true));
        else if(condition == MotorTestManager.Condition.baseMiddle)
            _playbackCoroutine = StartCoroutine(PlayVideo(new Sprite[] {_baseMiddleFrame, _baseMiddleFrame, _baseMiddleFrame}, _baseMiddleFrame, true));
    }

    public void Stop()
    {
        _frameImage.enabled = false;
        if(_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
    }
    
    private IEnumerator PlayVideo(Sprite[] video, Sprite baseFingerFrame, bool baseStimulus = false)
    {
        _frameImage.enabled = true;

        //Show hand without a cue for 1232 ms
        _frameImage.sprite = _baseFrame;
            
        yield return new WaitForSeconds(1.232f);
        
        //start timer
        MotorTestManager.instance.OnTimerStart();
        
        if (!baseStimulus)
        {
            foreach (Sprite frame in video)
            {
                _frameImage.sprite = frame;
                yield return new WaitForSeconds(frameTime);
            }
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            //Show the picture with the cue on the hand
            _frameImage.sprite = baseFingerFrame;
            yield return new WaitForSeconds(1.5f);
        }
    }
    
}
