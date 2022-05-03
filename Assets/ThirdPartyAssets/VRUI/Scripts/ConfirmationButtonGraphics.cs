using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{
    public Material buttonOff, buttonOn;

    private float _currentScale;
    [SerializeField] private float _scaleTarget;

    private float _velocity;
    
    private bool _loopAnimation = true;
    private bool _buttonIsOn = false;
    
    private Coroutine _idleCoroutine;

    [SerializeField] private float _scaleAmount;
    [SerializeField] private float _dampTime;
    [SerializeField] private float _delay;
    
    private void Start()
    {
        _idleCoroutine = StartCoroutine(AnimateButton());
    }

    private void OnEnable()
    {
        _idleCoroutine = StartCoroutine(AnimateButton());
        _loopAnimation = true;
    }

    private void Update()
    {
        _currentScale = LeanSmooth.damp(_currentScale, _scaleTarget, ref _velocity, _dampTime);
        transform.localScale = new Vector3(_currentScale, _currentScale, _currentScale);
    }

    public void SwitchSelection(bool on)
    {
        _buttonIsOn = on;
        if (on) {
            GetComponent<MeshRenderer>().material = buttonOn;
            _loopAnimation = false;
            StopCoroutine(_idleCoroutine);
            _scaleTarget = _scaleAmount * 1.2f;
        }
        else {
            GetComponent<MeshRenderer>().material = buttonOff;
            if (gameObject.activeSelf)
            {
                _loopAnimation = true;
                _idleCoroutine = StartCoroutine(AnimateButton());
            }
        }
    }
    
    private IEnumerator AnimateButton(bool fromOn = false) {
        if(!_buttonIsOn) _scaleTarget = 1;

        yield return new WaitForSeconds(_delay);

        if(!_buttonIsOn) _scaleTarget = _scaleAmount;

        yield return new WaitForSeconds(_delay);
        
        if(_loopAnimation)
            StartCoroutine(AnimateButton());
    }
}
