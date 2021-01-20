using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCurtainCanvasController : MonoBehaviour
{
    public static OpenCurtainCanvasController instance;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void Show(bool show, string message = "")
    {
        
       var to = 0;
       if (show) to = 1;

       GetComponentInChildren<Text>().text = message;
       var canvasGroup = GetComponent<CanvasGroup>();
       
        LeanTween.value(gameObject, canvasGroup.alpha,  to, 1).setOnUpdate((val) => { canvasGroup.alpha = val; });
    }
    
    //close curtain on start
}
