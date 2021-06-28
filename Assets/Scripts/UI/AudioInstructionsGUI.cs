using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioInstructionsGUI : MonoBehaviour
{
    private Button _audioButtons;

    public void ButtonPressed(int id)
    {
        AudioManager.instance.PlaySound(id);
    }

    public void LanguageChanged(string language)
    {
        LocalizationManager.instance.LoadLocalizedText(language);
    }
}
