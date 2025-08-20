using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using ScriptableObjectArchitecture;
using UnityEngine;

public class JsonSequenceController : MonoBehaviour
{
    public delegate void OnHidePanel();
    public static OnHidePanel HidePanel;

    [Header("Sequence Source")]
    [SerializeField] private SequenceData sequenceData;

    [Header("Settings")]
    [SerializeField] private string languageCode = "en";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Events")]
    [SerializeField] private BoolVariable _experienceRunning;
    [SerializeField] private StringGameEvent setInstructionsTextGameEvent;

    [Header("Visuals")]
    [SerializeField] private VisualPlayer visualPlayer;

    [SerializeField] private StatusManager _statusManager;

    private Sequence _dotweenSequence;

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
        if (sequenceData == null || _experienceRunning.Value) Debug.Log("not starting sequence"); //TODO needed?

        Debug.Log("starting sequence");
        
        _experienceRunning.Value = true;
        _dotweenSequence = DOTween.Sequence();

        float lastTime = 0f;
        Debug.Log("sequencing steps");
        foreach (var step in sequenceData.steps)
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
        //if (!_experienceRunning.Value) return; //TODO needed?

        if (_dotweenSequence != null && _dotweenSequence.IsActive())
        {
            _dotweenSequence.Kill();
        }

        audioSource.Stop();
        visualPlayer.Hide();

        _experienceRunning.Value = false;
    }

    private void EndSequence() //TODO do we need End Sequence and Stop Sequence?
    {
        //if (!_experienceRunning.Value) return;  //TODO needed?

        _experienceRunning.Value = false;
    }

    private void ExecuteStep(SequenceStep step)
    {
        Debug.Log("executing step");
        PrintStep(step);
        
        if (!string.IsNullOrEmpty(step.textKey)) setInstructionsTextGameEvent?.Raise(step.textKey);

        if (!string.IsNullOrEmpty(step.audio))
        {
            string audioPath = Path.Combine(Application.dataPath, $"Content/Sequence/Audio/{languageCode}", step.audio);
            StartCoroutine(LoadAndPlayAudio(audioPath));
        }

        if (!string.IsNullOrEmpty(step.visual)) visualPlayer.Show(step.visual);

        if (step.actions != null && step.actions.Count > 0) 
        {
            foreach (var action in step.actions)
            {
                switch (action)
                {
                    case "HidePanel":
                        HidePanel();
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
                        //_statusManager.CloseWall();
                        break;
                    case "HideVisual":
                        //_statusManager.WallOff();
                        break;
                }
            }
        }
    }

    private IEnumerator LoadAndPlayAudio(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Audio file not found: {fullPath}");
            yield break;
        }

        string url = "file://" + fullPath;
        using var www = new WWW(url);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            audioSource.clip = www.GetAudioClip();
            audioSource.Play();
        }
        else
        {
            Debug.LogError($"Failed to load audio: {www.error}");
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
