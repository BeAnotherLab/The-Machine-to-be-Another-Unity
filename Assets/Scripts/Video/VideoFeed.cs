using System;
using System.Collections;
using Mirror.Examples.Pong;
using UnityEngine;
using UnityEngine.Serialization;

public class VideoFeed : MonoBehaviour //TODO turn to manager
{
    #region Public Fields
    
    [HideInInspector]
    public Transform targetTransform;
    #endregion


    #region Private Fields

    [SerializeField] private bool _loadTiltFromPlayerPrefs = true;
    [SerializeField] private float _tiltAngle;

    private float _turningRate = 90f;
    private bool _dimmed = true;

    #endregion


    #region MonoBehaviour Methods

    private void OnEnable()
    {
        SwapControlGUI.DimButtonOn += Dim;
        SettingsGUI.ToggleDim += ToggleDim;
        SettingsGUI.RotateCamera += Rotate;
        CustomPlayer.SignalingSelf += GetPlayerTransform;
    }

    private void OnDisable()
    {
        SwapControlGUI.DimButtonOn -= Dim;
        SettingsGUI.ToggleDim -= ToggleDim;
        SettingsGUI.RotateCamera -= Rotate;
        CustomPlayer.SignalingSelf -= GetPlayerTransform;
    }

    private void Start()
    {
        if (_loadTiltFromPlayerPrefs) _tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
    }

    private void Update()    
    {
        if (Input.GetKeyDown("b")) ToggleDim(); //TODO move to unified keyboard input script (settings gui..)
        if (Input.GetKeyDown("r")) Rotate();
    }

    #endregion


    #region Public Methods

    public void Dim(bool dim) 
    {
        if (targetTransform != null)
        {
            float next = 1;
            if (dim) next = 0;
            float dimValue = targetTransform.GetComponentInChildren<MeshRenderer>().material.color.a;
            float time = 1;
            
            LeanTween.value(dimValue, next, time).setEaseInOutQuad().setOnUpdate((val) => {
                if (targetTransform != null)
                {
                    Color c = targetTransform.GetComponentInChildren<MeshRenderer>().material.color;
                    c.a = val;
                    targetTransform.GetComponentInChildren<MeshRenderer>().material.SetColor("_Color", c);                    
                }
            });    
        }
    }

    #endregion

    
    #region Private Methods
    
    private void ToggleDim()
    {
        _dimmed = !_dimmed;
        Dim(_dimmed);
    }

    private void Rotate()
    {
        _tiltAngle += 90;
        PlayerPrefs.SetFloat("tiltAngle", _tiltAngle);
    }

    #endregion

    
    private void GetPlayerTransform(Transform playerTransform)
    {
        targetTransform = playerTransform;
    }
}
