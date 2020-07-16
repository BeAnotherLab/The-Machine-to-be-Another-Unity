using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using RenderHeads.Media.AVProLiveCamera;

public class ExperimentSettingsGUI : MonoBehaviour
{
    //TODO when setting familiarization, disable ip address and participant
    public static ExperimentSettingsGUI instance;
    
    //Experiment settings
    [SerializeField] private InputField _subjectInputField;
    [SerializeField] private Dropdown _taskCounterbalancingDropdown;
    [SerializeField] private Dropdown _conditionDropdown;
    [SerializeField] private Dropdown _participantDropdown;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    [SerializeField] private GameObject _videoNotFoundErrorMessage;
    [SerializeField] private Dropdown _directionDropdown;
    [SerializeField] private List<String> tasks = new List<string>();
    
    private void Awake()
    {
        if (instance == null) instance = this;

        _subjectInputField.onEndEdit.AddListener( delegate {
            FamiliarizationManager.instance.SetSubjectID(_subjectInputField.text);
        });
        
        _taskCounterbalancingDropdown.onValueChanged.AddListener(delegate(int arg0)
        {
            FamiliarizationManager.instance.SelectOrder(_taskCounterbalancingDropdown.options[arg0].text);
        });
        
        _startButton.onClick.AddListener(delegate
        {
            FamiliarizationManager.instance.StartExperiment(
                _conditionDropdown.options[_conditionDropdown.value].text,
                _participantDropdown.options[_participantDropdown.value].text,
                _subjectInputField.text, 
                _directionDropdown.options[_directionDropdown.value].text);
        });

        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
    }

    private void Start()
    {
        List<string> _dropDownOptions = new List<string>();

        foreach (List<string> permutation in Permutate(tasks, tasks.Count)) {
            string _option = string.Join(" ", permutation.ToArray());
            _dropDownOptions.Add(_option);
        }

        _taskCounterbalancingDropdown.AddOptions(_dropDownOptions);    
    }

    public void NotifyVideoNotFoundError()
    {
        StartCoroutine(ShowAndHideVideoNotFoundError());
    }
    
    public void ShowExistingSubjectIDError()
    {
        StartCoroutine(ShowAndHideExistingSubjectIDError());
    }
    
    private IEnumerator ShowAndHideExistingSubjectIDError()
    {
        _subjectExistingErrorMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        _subjectExistingErrorMessage.gameObject.SetActive(false);
    }
    
    private IEnumerator ShowAndHideVideoNotFoundError()
    {
        _videoNotFoundErrorMessage.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        _videoNotFoundErrorMessage.gameObject.SetActive(false);
    }
    
    //from https://www.codeproject.com/Articles/43767/A-C-List-Permutation-Iterator
    private IEnumerable<IList> Permutate(List<string> sequence, int count) 
    {
        if (count == 1) yield return sequence;
        else {
            for (int i = 0; i < count; i++) {
                foreach (var perm in Permutate(sequence, count - 1))
                    yield return perm;
                RotateRight(sequence, count);
            }
        } 
    }

    private void RotateRight(List<string> sequence, int count) 
    {
        string tmp = sequence[count - 1];
        sequence.RemoveAt(count - 1);
        sequence.Insert(0, tmp);
    }

}
