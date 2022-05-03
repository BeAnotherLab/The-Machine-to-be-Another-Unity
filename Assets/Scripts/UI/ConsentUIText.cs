using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Localization;
using UnityEngine.UI;

public class ConsentUIText : MonoBehaviour //TODO use inheritence over composition
{
    [SerializeField] private Text _text;

    public void ConsentButtonPressed()
    {
        _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "waitForOther";
    }

    public void Show()
    {
        _text.gameObject.GetComponent<LeanLocalizedText>().TranslationName = "consent"; 
    }
}
