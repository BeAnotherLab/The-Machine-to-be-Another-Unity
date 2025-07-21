using System;
using System.Collections;
using Mirror.Examples.Pong;
using UnityEngine;
using UnityEngine.Serialization;

public class VideoFeed : MonoBehaviour //TODO turn to manager
{
    #region Public Fields
    
  
    [HideInInspector]
    public Quaternion otherPose; //TODO remove
    public int cameraID; //app must be reset for changes to be applied. first camera is for swap, second is for cognitive task
    public bool dimOnStart;
    public Transform targetTransform;
    #endregion


    #region Private Fields

    [SerializeField] private bool _loadTiltFromPlayerPrefs = true;
    [SerializeField] private bool _editing;
    
    //Camera params
    private float _turningRate = 90f;
    [SerializeField] private float _tiltAngle;

    //Dim params
    private bool _dimmed;

    private MeshRenderer _meshRenderer;

    
    #endregion


    #region MonoBehaviour Methods

    private void OnEnable()
    {
        SwapControlGUI.RecenterPoserButtonPressed += RecenterPose;
        SwapControlGUI.DimButtonOn += Dim;
        OscManager.ReceiveRecenterPose += RecenterPose;
        SettingsGUI.ToggleDim += ToggleDim;
        SettingsGUI.RotateCamera += Rotate;
        SettingsGUI.RecenterPose += RecenterPose;
        CustomPlayer.SignalingSelf += GetPlayerTransform;
        //StatusManager.Standby += DimOn; TODO assign
    }

    private void OnDisable()
    {
        SwapControlGUI.RecenterPoserButtonPressed -= RecenterPose;
        SwapControlGUI.DimButtonOn -= Dim;
        OscManager.ReceiveRecenterPose -= RecenterPose;
        SettingsGUI.ToggleDim -= ToggleDim;
        SettingsGUI.RotateCamera -= Rotate;
        SettingsGUI.RecenterPose -= RecenterPose;
        CustomPlayer.SignalingSelf -= GetPlayerTransform;
        //StatusManager.Standby -= DimOn; TODO assign
    }

    void Start()
    {
        if(_loadTiltFromPlayerPrefs) _tiltAngle = PlayerPrefs.GetFloat("tiltAngle");
        //if (dimOnStart) StartCoroutine(StartupDim()); TODO this is also done in player? whatfor?
        otherPose = new Quaternion();
    }

    // Update is called once per frame
    void Update()    
    {
        Quaternion nextOtherPose = new Quaternion();

        // Turn towards our target rotation. TODO remove
        otherPose = Quaternion.RotateTowards(otherPose, nextOtherPose, _turningRate * Time.deltaTime);

        if (Input.GetKeyDown("b") && !_editing ) ToggleDim();
        if (Input.GetKeyDown("n") && !_editing ) RecenterPose();
        if (Input.GetKeyDown("r") && !_editing ) Rotate();
    }

    void OnDestroy()
    {
        PlayerPrefs.SetInt("cameraID", cameraID);
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

    public void ToggleDim()
    {
        _dimmed = !_dimmed;
        Dim(_dimmed);
    }

    public void Rotate()
    {
        _tiltAngle += 90;
        PlayerPrefs.SetFloat("tiltAngle", _tiltAngle);
    }

    public void RecenterPose()
    {
        UnityEngine.XR.InputTracking.Recenter(); //TODO obsolete, replace
        //The following will also move the camera positional reference.
        //taken from https://forum.unity.com/threads/openvr-how-to-reset-camera-properly.417509/#post-2792972
    }
 
    #endregion

    private void GetPlayerTransform(Transform playerTransform)
    {
        targetTransform = playerTransform;
    }
}
