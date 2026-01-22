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

    private bool _dimmed = true;

    #endregion


    #region MonoBehaviour Methods

    private void OnEnable()
    {
        SettingsGUI.ToggleDim += ToggleDim;
        CustomPlayer.SignalingSelf += GetPlayerTransform;
    }

    private void OnDisable()
    {
        SettingsGUI.ToggleDim -= ToggleDim;
        CustomPlayer.SignalingSelf -= GetPlayerTransform;
    }

    private void Update()    
    {
        if (Input.GetKeyDown("b")) ToggleDim(); //TODO move to unified keyboard input script (settings gui..)
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

    #endregion

    
    private void GetPlayerTransform(Transform playerTransform)
    {
        targetTransform = playerTransform;
    }
}
