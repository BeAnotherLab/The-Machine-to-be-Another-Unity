using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{
    public Material buttonOff, buttonOn;
    public static ConfirmationButtonGraphics instance;
    
    private float _currentScale;
    [SerializeField] private float _scaleTarget;

    private float _velocity;
    
    private bool _loopAnimation = true;

    private Coroutine _idleCoroutine;

    [SerializeField] private float _scaleAmount;
    [SerializeField] private float _dampTime;
    [SerializeField] private float _delay;
    
    void Awake()
    {
        if (instance == null) instance = this;
    }

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

    public void SwitchSelection(bool _on) {
        if (_on) {
            GetComponent<MeshRenderer>().material = buttonOn;
            _loopAnimation = false;
            StopCoroutine(_idleCoroutine);
            Debug.Log("stop coroutine");
            _scaleTarget = _scaleAmount * 1.2f;
        }
        else {
            GetComponent<MeshRenderer>().material = buttonOff;
            if(gameObject.activeSelf)
              _idleCoroutine = StartCoroutine(AnimateButton(true));
            _loopAnimation = true;
        }
    }
    
    private IEnumerator AnimateButton(bool fromOn = false) {
        if (fromOn) _scaleTarget = 1;
        else _scaleTarget = _scaleAmount;
        
        yield return new WaitForSeconds(_delay);

        if (fromOn) _scaleTarget = _scaleAmount;
        else _scaleTarget = 1;

        yield return new WaitForSeconds(_delay);
        
        if(_loopAnimation)
            StartCoroutine(AnimateButton());
    }
}
