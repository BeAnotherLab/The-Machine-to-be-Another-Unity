using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFades : MonoBehaviour
{

    private void OnEnable()
    {
        LeanTween.alpha(gameObject, 0f, 0f).setEase(LeanTweenType.easeInCirc);
        LeanTween.alpha(gameObject, 1f, 0.5f).setEase(LeanTweenType.easeInCirc);
    }

    private void OnDisable()
    {
        LeanTween.alpha(gameObject, 0f, 0.5f).setEase(LeanTweenType.easeInCirc);
    }
}
