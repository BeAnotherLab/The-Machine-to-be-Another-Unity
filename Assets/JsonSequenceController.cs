using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.Serialization;

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
    public delegate void OnHidePanel();
    public static OnHidePanel HidePanel;

    [FormerlySerializedAs("sequenceData")]
    [Header("Sequence Source")]
    [SerializeField] private SequenceData _sequenceData;

    [FormerlySerializedAs("languageCode")]
    [Header("Settings")]
    [SerializeField] private string _languageCode = "en";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Events")]
    [SerializeField] private BoolVariable _experienceRunning;
    [FormerlySerializedAs("setInstructionsTextGameEvent")] [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;

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

    public void SwitchLanguageTrack(string language)
    {
        switch (language)
        {
            case "English":
                _languageCode = "EN";
                break;
            case "Italian":
                _languageCode = "IT";
                break;
            case "French":
                _languageCode = "FR";
                break;
            case "German":
                _languageCode = "DE";
                break;
        }
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

        audioSource.Stop();
        visualPlayer.Hide();

        _experienceRunning.Value = false;
    }

    private void EndSequence() //TODO do we need End Sequence and Stop Sequence?
    {
        _experienceRunning.Value = false;
    }

    private void ExecuteStep(SequenceStep step)
    {
        Debug.Log("executing step");
        PrintStep(step);
        
        if (!string.IsNullOrEmpty(step.textKey)) _setInstructionsTextGameEvent?.Raise(step.textKey);

        if (!string.IsNullOrEmpty(step.audio)) StartCoroutine(LoadAndPlayAudio(ContentPath.Audio(_languageCode, step.audio)));

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
