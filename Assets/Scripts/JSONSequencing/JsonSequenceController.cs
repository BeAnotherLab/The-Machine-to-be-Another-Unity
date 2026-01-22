using System;
using System.Collections;
using System.IO;
using DG.Tweening;
using Newtonsoft.Json;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngineInternal;

/*
 The JsonSequenceController replaces Unity's Timeline system with a custom timeline runner that:
Reads a SequenceData ScriptableObject (your timeline).
Plays audio, shows visuals (images or videos), and triggers actions at specified times.
    Starts the sequence when both users are ready.
    Stops if there's a serial failure or if a user leaves.
    Handles localization (by switching language folders for audio)
*/

public class JsonSequenceController : MonoBehaviour
{
    public delegate void OnShowPanel();
    public static OnShowPanel ShowPanel;
    
    public delegate void OnHidePanel();
    public static OnHidePanel HidePanel;

    public delegate void OnLoadVisual(string filename);
    public static OnLoadVisual LoadVisual;

    public delegate void OnShowVisual();
    public static OnShowVisual ShowVisual;
    
    public delegate void OnHideVisual();
    public static OnHideVisual HideVisual;
    
    public delegate void OnPlayAudio(string key);
    public static OnPlayAudio PlayAudio;
    
    [Header("Sequence Source")]
    [SerializeField] private SequenceData _sequenceData;
    
    [Header("Events")]
    [SerializeField] private BoolVariable _experienceRunning;
    [SerializeField] private StringGameEvent _setInstructionsTextFromKeyGameEvent;

    private StatusManager _statusManager;

    private Sequence _dotweenSequence;
    
    [SerializeField] private Translations _translations;
        
    private void OnEnable()
    {
        UserStateManager.BothUsersReady += StartSequence;
        ArduinoManager.SerialFailure += StopSequence;
        OscManager.ReceiveSerialFailure += StopSequence;
        UserStateManager.OtherLeft += StopSequence;
        UserStateManager.ThisUserLeft += StopSequence;
    }

    private void OnDisable()
    {
        UserStateManager.BothUsersReady -= StartSequence;
        ArduinoManager.SerialFailure -= StopSequence;
        OscManager.ReceiveSerialFailure -= StopSequence;
        UserStateManager.OtherLeft -= StopSequence;
        UserStateManager.ThisUserLeft -= StopSequence;
    }

    private void StartSequence()
    {
        _experienceRunning.Value = true;
        _dotweenSequence = DOTween.Sequence();

        float lastTime = 0f;
        foreach (var step in _sequenceData.steps)
        {
            PrintStep(step);
            
            float delay = step.time - lastTime;
            lastTime = step.time;

            var capturedStep = step;

            _dotweenSequence.AppendInterval(delay)
                            .AppendCallback(() => ExecuteStep(capturedStep));
        }

        _dotweenSequence.OnComplete(EndSequence);
    }

    private void StopSequence()
    {
        if (_dotweenSequence != null && _dotweenSequence.IsActive()) _dotweenSequence.Kill();

        //TODO notify sequence stopped?
        HideVisual();

        _experienceRunning.Value = false;
    }

    private void EndSequence() //TODO do we need End Sequence and Stop Sequence?
    {
        _experienceRunning.Value = false;
    }

    private void ExecuteStep(SequenceStep step)
    {
        PrintStep(step);

        if (!string.IsNullOrEmpty(step.textKey)) _setInstructionsTextFromKeyGameEvent.Raise(step.textKey);

        if (!string.IsNullOrEmpty(step.audio)) PlayAudio(step.audio); 

        if (!string.IsNullOrEmpty(step.visual)) LoadVisual(step.visual);

        if (step.actions != null && step.actions.Count > 0)
        {
            foreach (var action in step.actions)
            {
                switch (action)
                {
                    case "HidePanel":
                        HidePanel();
                        break;
                    case "ShowPanel":
                        ShowPanel();
                        break;
                    case "StartExperience":
                        _statusManager.StartExperience();
                        break;
                    case "EndExperience":
                        _statusManager.EndExperience();
                        break;
                    case "MirrorOn":
                        _statusManager.MirrorOn();
                        break;
                    case "MirrorOff":
                        _statusManager.MirrorOff();
                        break;
                    case "WallOn":
                        _statusManager.CloseWall();
                        break;
                    case "WallOff":
                        _statusManager.WallOff();
                        break;
                    case "ShowVisual":
                        ShowVisual();
                        break;
                    case "HideVisual":
                        HideVisual();
                        break;
                }
            }
        }
    }
    
    
    private void PrintStep(SequenceStep step)
    {
        Debug.Log($"<b><color=#888>[Step @ {step.time}s]</color></b> " +
                  $"Text: <color=#4CAF50>{(string.IsNullOrEmpty(step.textKey) ? "-" : step.textKey)}</color>, " +
                  $"Audio: <color=#2196F3>{(string.IsNullOrEmpty(step.audio) ? "-" : step.audio)}</color>, " +
                  $"Visual: <color=#FF9800>{(string.IsNullOrEmpty(step.visual) ? "-" : step.visual)}</color>, " +
                  $"Actions: <color=#E91E63>[{(step.actions != null && step.actions.Count > 0 ? string.Join(", ", step.actions) : "-")}]</color>");
    }
    
    
}
