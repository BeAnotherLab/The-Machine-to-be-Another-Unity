using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionnaireToggleGroup : MonoBehaviour
{
    [SerializeField] private Button _nextButton;
    [SerializeField] private CanvasGroup _canvasGroup; 
        
    public void ReadyToShowQuestionnaire()
    {
        _nextButton.gameObject.SetActive(false);
        GetComponent<ToggleGroup>().SetAllTogglesOff();
        _nextButton.interactable = false;
        _nextButton.gameObject.SetActive(true);
    }

    public void ToggleChanged()
    {
        if (_canvasGroup.alpha == 1)
            _nextButton.interactable = true;
    }
    
}
