using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardControls : MonoBehaviour
{
    [SerializeField] private Button _curtainUpButton;
    [SerializeField] private Button _curtainDownButton;
    [SerializeField] private Button _screenOnButton;
    [SerializeField] private Button _screenOffButton;
    [SerializeField] private Button _leaderButton;
    [SerializeField] private Button _followerButton;
    [SerializeField] private Button _freeButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _questionnaireOnButton;
    [SerializeField] private Button _questionnaireOffButton;


    public Transform transition;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _curtainUpButton.onClick.Invoke();
            AnimateButton(_curtainUpButton);
        }   
        if (Input.GetKeyDown(KeyCode.O))
        {
            _curtainDownButton.onClick.Invoke();
            AnimateButton(_curtainDownButton);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            _screenOnButton.onClick.Invoke();
            AnimateButton(_screenOnButton);

        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            _screenOffButton.onClick.Invoke();
            AnimateButton(_screenOffButton);

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _leaderButton.onClick.Invoke();
            AnimateButton(_leaderButton);

        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _followerButton.onClick.Invoke();
            AnimateButton(_followerButton);

        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            _freeButton.onClick.Invoke();
            AnimateButton(_freeButton);

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _startButton.onClick.Invoke();
            AnimateButton(_startButton);

        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            _questionnaireOnButton.onClick.Invoke();
            AnimateButton(_questionnaireOnButton);

        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            _questionnaireOffButton.onClick.Invoke();
            AnimateButton(_questionnaireOffButton);
        }
    }
    
    
    private void AnimateButton(Button button)
    {
        StartCoroutine(FullAnimation(button));
    }
    
    private IEnumerator FullAnimation (Button button)
    {
        button.image.CrossFadeColor(button.colors.pressedColor, button.colors.fadeDuration, true, true);
        yield return new WaitForSeconds(0.3f);
        button.image.CrossFadeColor(button.colors.normalColor, button.colors.fadeDuration, true, true);
    }

}
