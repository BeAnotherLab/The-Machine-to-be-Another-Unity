using System.Collections;
using System.Collections.Generic;
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

    public delegate void OnInstructionPlaying();
    public static OnInstructionPlaying InstructionPlaying;
    
    public delegate void OnInstructionFinished();
    public static OnInstructionFinished InstructionFinished;
    
    [Header("Sequence Source")]
    [SerializeField] private SequenceData _sequenceData;

    [Header("Settings")]
    [SerializeField] private string _languageCode = "en"; //TODO make sure this gets set by lean localization before 1st run

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;

    [Header("Events")]
    [SerializeField] private BoolVariable _experienceRunning;
    [SerializeField] private StringGameEvent _setInstructionsTextGameEvent;

    [SerializeField] private StatusManager _statusManager;

    private Sequence _dotweenSequence;
    private Dictionary<string, string> _translations;

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

    public void SwitchLanguageTrack(string language) //TODO shouldn't use plain english name, just language code
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

        LoadTranslations(_languageCode);
    }

    private void LoadTranslations(string languageCode)
    {
        string path = ContentPath.Translation(languageCode);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Translation file not found at: {path}");
            _translations = new Dictionary<string, string>();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            _translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading translations from {path}: {e.Message}");
            _translations = new Dictionary<string, string>();
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

        _audioSource.Stop();
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

        if (!string.IsNullOrEmpty(step.textKey))
        {
            if (_translations != null && _translations.TryGetValue(step.textKey, out string translatedText))
            {
                _setInstructionsTextGameEvent.Raise(translatedText);
            }
            else
            {
                Debug.LogWarning($"Missing translation for key: {step.textKey}");
                _setInstructionsTextGameEvent.Raise(step.textKey); // fallback
            }
        }

        if (!string.IsNullOrEmpty(step.audio)) StartCoroutine(LoadAndPlayAudio(ContentPath.Audio(_languageCode, step.audio)));

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
    
    private IEnumerator LoadAndPlayAudio(string fullPath)
    {
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning($"Audio file not found: {fullPath}");
            yield break;
        }

        string extension = Path.GetExtension(fullPath).ToLower();
        AudioType audioType = extension switch
        {
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            _ => AudioType.UNKNOWN
        };

        if (audioType == AudioType.UNKNOWN)
        {
            Debug.LogError($"Unsupported audio format: {extension}");
            yield break;
        }

        string url = "file://" + fullPath;
        Debug.Log($"Loading audio: {url} as {audioType}");

        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
    if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError($"Failed to load audio: {www.error}");
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
        if (clip == null)
        {
            Debug.LogError("AudioClip is null after loading.");
            yield break;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
        StartCoroutine(WaitForAudioEnd());
    }
    
    private void PrintStep(SequenceStep step)
    {
        Debug.Log($"<b><color=#888>[Step @ {step.time}s]</color></b> " +
                  $"Text: <color=#4CAF50>{(string.IsNullOrEmpty(step.textKey) ? "-" : step.textKey)}</color>, " +
                  $"Audio: <color=#2196F3>{(string.IsNullOrEmpty(step.audio) ? "-" : step.audio)}</color>, " +
                  $"Visual: <color=#FF9800>{(string.IsNullOrEmpty(step.visual) ? "-" : step.visual)}</color>, " +
                  $"Actions: <color=#E91E63>[{(step.actions != null && step.actions.Count > 0 ? string.Join(", ", step.actions) : "-")}]</color>");
    }
    
    private IEnumerator WaitForAudioEnd()
    {
        InstructionPlaying();
        yield return new WaitWhile(() => _audioSource.isPlaying);
        InstructionFinished();
    }
    
}
