using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class ConsentUI : MonoBehaviour
{
    [SerializeField] private GameObject _yesButton;
    [SerializeField] private GameObject _noButton;
    [SerializeField] private GameObject _textPanel;
    
    public void ReadyToShowQuestionnaire()
    {
        Show(false);
    }

    public void Show(bool show)
    {
        _yesButton.gameObject.SetActive(show); 
        _noButton.gameObject.SetActive(show);
        _textPanel.GetComponent<PanelDimmer>().Show(show, 1f);
    }
}
