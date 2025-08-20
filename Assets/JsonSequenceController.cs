using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ScriptableObjectArchitecture;
using UnityEngine;
/*The JsonSequenceController (renamed from your old TimelineController) replaces Unity's Timeline system with a custom timeline runner that:

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
        if (sequenceData == null || _experienceRunning.Value) return; //TODO needed?

        _experienceRunning.Value = true;

        for (int i = 0; i < sequenceData.steps.Count; i++)
        {
            var step = sequenceData.steps[i];
            float delay = step.time;

            LeanTween.delayedCall(gameObject, delay, () => ExecuteStep(step)).setOnComplete(() =>
            {
                if (step == sequenceData.steps[sequenceData.steps.Count - 1]) EndSequence();
            });
        }
    }

    private void StopSequence()
    {
        if (!_experienceRunning.Value) return; //TODO needed?

        LeanTween.cancel(gameObject);
        audioSource.Stop();
        visualPlayer.Hide();

        _experienceRunning.Value = false;
    }

    private void EndSequence() //TODO do we need End Sequence and Stop Sequence?
    {
        if (!_experienceRunning.Value) return;  //TODO needed?

        _experienceRunning.Value = false;
    }

    private void ExecuteStep(SequenceStep step)
    {
        if (!string.IsNullOrEmpty(step.textKey)) setInstructionsTextGameEvent?.Raise(step.textKey);

        if (!string.IsNullOrEmpty(step.audio))
        {
            string audioPath = Path.Combine(Application.dataPath, $"Content/Sequence/Audio/{languageCode}", step.audio);
            StartCoroutine(LoadAndPlayAudio(audioPath));
        }

        if (!string.IsNullOrEmpty(step.visual)) visualPlayer.Show(step.visual);

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
}
