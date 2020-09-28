using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFades : MonoBehaviour
{
    private Color _color;


    private void Awake()
    {
        _color = this.gameObject.GetComponent<Renderer>().material.color;
    }


    private void OnEnable()
    {
        //LeanTween.alpha(this.gameObject, 1f, 0.5f).setEase(LeanTweenType.easeInCirc);
        StartCoroutine(Fade(true));
        Debug.Log("enable fade");
    }

    private void OnDisable()
    {
        //LeanTween.alpha(this.gameObject, 0f, 0.5f).setEase(LeanTweenType.easeInCirc);
        StartCoroutine(Fade(false));
        Debug.Log("disable fade");
    }

    private IEnumerator Fade(bool _in){
        float elapsedTime = 0f;

        if( _in){
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                _color = new Color (_color.r, _color.g, _color.b, elapsedTime / 1f);
                this.gameObject.GetComponent<Renderer>().material.color = _color;
                yield return null;
            }
        }

        else
            while (elapsedTime < 1f)
            {
                elapsedTime += Time.deltaTime;
                _color = new Color(_color.r, _color.g, _color.b, 1-(elapsedTime / 1f));
                this.gameObject.GetComponent<Renderer>().material.color = _color;
                yield return null;
            }
    }
}
