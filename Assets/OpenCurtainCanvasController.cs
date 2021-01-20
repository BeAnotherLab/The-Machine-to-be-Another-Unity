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

    public void Show(string message = "")
    {
       GetComponentInChildren<Text>().text = message;
       var canvasGroup = GetComponent<CanvasGroup>();
           
       var seq = LeanTween.sequence();
       seq.append( 
           LeanTween.value(gameObject, 0, 1, 1).setOnUpdate((val) => { canvasGroup.alpha = val; })
       );
       seq.append(30f); 
       seq.append(
           LeanTween.value(gameObject, 1, 0, 1).setOnUpdate((val) => { canvasGroup.alpha = val; })
           ); // do a tween

    }
    
}
