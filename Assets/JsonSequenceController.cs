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
    [Header("Sequence Source")]
    [SerializeField] private SequenceData sequenceData;

    [Header("Settings")]
    [SerializeField] private string languageCode = "en";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Events")]
    [SerializeField] private BoolVariable _experienceRunning;
    [SerializeField] private GameEvent startExperienceEvent;
    [SerializeField] private GameEvent endExperienceEvent;
    [SerializeField] private StringGameEvent setInstructionsTextGameEvent;
    [SerializeField] private GameEvent hidePanelEvent;

    [Header("Visuals")]
    [SerializeField] private VisualPlayer visualPlayer;

    private int tweenId = -1;
    private bool isRunning;

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
        if (sequenceData == null || isRunning) return;

        isRunning = true;
        _experienceRunning.Value = true;
        startExperienceEvent?.Raise();

        float startTime = Time.time;

        for (int i = 0; i < sequenceData.steps.Count; i++)
        {
            var step = sequenceData.steps[i];
            float delay = step.time;

            LeanTween.delayedCall(gameObject, delay, () => ExecuteStep(step)).setOnComplete(() =>
            {
                if (step == sequenceData.steps[sequenceData.steps.Count - 1])
                {
                    EndSequence();
                }
            });
        }
    }

    private void StopSequence()
    {
        if (!isRunning) return;

        LeanTween.cancel(gameObject);
        audioSource.Stop();
        visualPlayer.Hide();

        _experienceRunning.Value = false;
        isRunning = false;
    }

    private void EndSequence()
    {
        if (!isRunning) return;

        _experienceRunning.Value = false;
        endExperienceEvent?.Raise();
        isRunning = false;
    }

    private void ExecuteStep(SequenceStep step)
    {
        if (!string.IsNullOrEmpty(step.textKey))
            setInstructionsTextGameEvent?.Raise(step.textKey);

        if (!string.IsNullOrEmpty(step.audio))
        {
            string audioPath = Path.Combine(Application.dataPath, $"Content/Sequence/Audio/{languageCode}", step.audio);
            StartCoroutine(LoadAndPlayAudio(audioPath));
        }

        if (!string.IsNullOrEmpty(step.visual))
        {
            visualPlayer.Show(step.visual);
        }

        foreach (var action in step.actions)
        {
            switch (action)
            {
                case "HidePanel":
                    hidePanelEvent?.Raise();
                    break;
                case "StartExperience":
                    startExperienceEvent?.Raise();
                    break;
                case "EndExperience":
                    endExperienceEvent?.Raise();
                    break;
                case "MirrorOn":
                    StatusManager.SendArduinoCommand?.Invoke("mir_on");
                    break;
                case "MirrorOff":
                    StatusManager.SendArduinoCommand?.Invoke("mir_off");
                    break;
                case "WallOn":
                    //GameEventsLibrary.Instance.CurtainOnEvent.Raise(true);
                    break;
                case "WallOff":
                    //GameEventsLibrary.Instance.CurtainOnEvent.Raise(false);
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
