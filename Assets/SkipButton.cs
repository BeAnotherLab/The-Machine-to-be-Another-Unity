﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkipButton : MonoBehaviour
{
    [SerializeField] private Button _skipButton;
    [SerializeField] private ExperimentData _experimentData;

    // Start is called before the first frame update
    void Awake()
    {
        _skipButton.onClick.AddListener(delegate { _experimentData.LoadNextScene(); });
    }
}
