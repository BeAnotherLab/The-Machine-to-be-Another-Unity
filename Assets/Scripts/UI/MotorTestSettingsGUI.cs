using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MotorTestSettingsGUI : MonoBehaviour
{
    [SerializeField] private InputField _subjectIDInputField;
    [SerializeField] private GameObject _subjectExistingErrorMessage;
    [SerializeField] private Dropdown _prePostDropdown;
    [SerializeField] private Button _startButton;

    public static MotorTestSettingsGUI instance;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
       
        _startButton.onClick.AddListener(delegate
        {
            MotorTestManager.instance.StartInstructions(
                _subjectIDInputField.text,
                _prePostDropdown.options[_prePostDropdown.value].text
            );
        });
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
