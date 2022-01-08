using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelDimmer : MonoBehaviour
{

    public void Show()
    {
        Show(true, 1f);
    }

    public void Hide()
    {
        Show(false, 0f);
    }   
    
    public void Show(bool show)
    {
        Show(show, 1f);
    }

    public void Show(bool show, float opacity, float time = 0.7f, bool loop = false, float delay = 0f)
    {
        var from = GetComponent<CanvasGroup>().alpha;
        var to = 0f;
        if (show) to = opacity;
        
        var loopType = LeanTweenType.once;
        if (loop) loopType = LeanTweenType.pingPong;

        if(!loop) GetComponent<CanvasGroup>().blocksRaycasts = show;
        
        LeanTween.value(gameObject, from, to, time )
            .setOnUpdate(delegate(float val)
            {
                GetComponent<CanvasGroup>().alpha = val;
            })
            .setLoopCount(2)
            .setLoopType(loopType)
            .setDelay(delay)
            .setEaseOutCubic();
    }
    
}