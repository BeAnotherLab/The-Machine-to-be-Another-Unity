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
    [SerializeField] private Button _noVRButton;
    [SerializeField] private Button _VRButton;
    [SerializeField] private Button _questionnaireOnButton;
    [SerializeField] private Button _questionnaireOffButton;
    [SerializeField] private Button _prepareExperimentButton;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _curtainUpButton.onClick.Invoke();
            AnimateButton(_curtainUpButton);
        }   
        else if (Input.GetKeyDown(KeyCode.O))
        {
            _curtainDownButton.onClick.Invoke();
            AnimateButton(_curtainDownButton);
        }
        else if (Input.GetKeyDown(KeyCode.N))
        {
            _screenOnButton.onClick.Invoke();
            AnimateButton(_screenOnButton);

        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            _screenOffButton.onClick.Invoke();
            AnimateButton(_screenOffButton);

        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            _leaderButton.onClick.Invoke();
            AnimateButton(_leaderButton);

        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            _followerButton.onClick.Invoke();
            AnimateButton(_followerButton);

        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            _freeButton.onClick.Invoke();
            AnimateButton(_freeButton);

        }
        else if (Input.GetKeyDown(KeyCode.Space)) // TODO change
        {
            _startButton.onClick.Invoke();
            AnimateButton(_startButton);

        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _noVRButton.onClick.Invoke();
            AnimateButton(_noVRButton);

        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _VRButton.onClick.Invoke();
            AnimateButton(_VRButton);
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            _questionnaireOnButton.onClick.Invoke();
            AnimateButton(_questionnaireOnButton);

        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            _questionnaireOffButton.onClick.Invoke();
            AnimateButton(_questionnaireOffButton);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            _prepareExperimentButton.onClick.Invoke();
            AnimateButton(_prepareExperimentButton);
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
