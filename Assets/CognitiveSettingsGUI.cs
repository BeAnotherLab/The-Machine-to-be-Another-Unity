using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CognitiveSettingsGUI : MonoBehaviour
{
    //Cognitive settings
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    [SerializeField] private Dropdown _pronounDropdown;
    [SerializeField] private Dropdown _prePostDropdown;
    [SerializeField] private Dropdown _directionDropdown;
    [SerializeField] private Toggle _showDummyToggle;
    [SerializeField] private Button _rotateButton;
    [SerializeField] private Button _startButton;

    public static CognitiveSettingsGUI instance;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
        
        _showDummyToggle.onValueChanged.AddListener(delegate(bool value){
            ShowDummy.instance.Show(value);
            _rotateButton.interactable = !value;
            VideoFeed.instance.gameObject.SetActive(!value);
            _directionDropdown.interactable = !value;
        });

        _startButton.onClick.AddListener(delegate
        {
            CognitiveTestManager.instance.StartInstructions(
                _pronounDropdown.options[_pronounDropdown.value].text,
                _subjectIDInputField.text,
                _directionDropdown.options[_directionDropdown.value].text,
                _prePostDropdown.options[_prePostDropdown.value].text
                );
        });
        
        _rotateButton.onClick.AddListener(delegate { VideoFeed.instance.Rotate(); });
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

}
