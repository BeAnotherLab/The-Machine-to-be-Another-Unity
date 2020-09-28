using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    #region Public Fields
    public GameObject image1, image2, panel1, panel2, text;
    #endregion

    #region MonoBehaviour Methods
    private void Start()
    {
        FadeInPanel();
        FadeInText();
    }
    #endregion

    #region Public Methods
    public void FadeInImages(){
        StartCoroutine(Fade(true, image1));
        StartCoroutine(Fade(true, image2));
    }

    public void FadeOutImages()
    {
        StartCoroutine(Fade(false, image1));
        StartCoroutine(Fade(false, image2));
    }

    public void FadeInPanel()
    {
        StartCoroutine(Fade(true, panel1));
        StartCoroutine(Fade(true, panel2));

    }

    public void FadeOutPanel()
    {
        StartCoroutine(Fade(false, panel1));
        StartCoroutine(Fade(false, panel2));
    }

    public void FadeInText()
    {
        StartCoroutine(Fade(true, text));
    }

    public void FadeOutText()
    {
        StartCoroutine(Fade(false, text));
    }

    public void FadeOutAll()
    {
        StartCoroutine(Fade(false, image1));
        StartCoroutine(Fade(false, image2));
        StartCoroutine(Fade(false, panel1));
        StartCoroutine(Fade(false, panel2));
        StartCoroutine(Fade(false, text));
    }

    public void FadeInAll()
    {
        StartCoroutine(Fade(true, image1));
        StartCoroutine(Fade(true, image2));
        StartCoroutine(Fade(true, panel1));
        StartCoroutine(Fade(true, panel2));
        StartCoroutine(Fade(true, text));
    }
    #endregion

    #region Private Fields

    public IEnumerator Fade(bool _in, GameObject _object)
    {
        Color _color = new Color();

        if(_object.GetComponent<Renderer>() != null)
            _color = _object.GetComponent<Renderer>().material.color;

        else if(_object.GetComponent<SpriteRenderer>() != null)
            _color = _object.GetComponent<SpriteRenderer>().material.color;

        else if (_object.GetComponent<Image>() != null)
            _color = _object.GetComponent<Image>().material.color;

        float elapsedTime = 0f;

            if (_in)
                while (elapsedTime < 1f)
                {
                    elapsedTime += Time.deltaTime;
                    _color = new Color(_color.r, _color.g, _color.b, elapsedTime / 1f);
                if (_object.GetComponent<Renderer>() != null)
                   _object.GetComponent<Renderer>().material.color = _color;
                else if (_object.GetComponent<SpriteRenderer>() != null)
                    _object.GetComponent<SpriteRenderer>().material.color = _color;
                else if (_object.GetComponent<Image>() != null)
                    _object.GetComponent<Image>().material.color = _color;

                yield return null;
                }

            else
                while (elapsedTime < 1f)
                {
                    elapsedTime += Time.deltaTime;
                    _color = new Color(_color.r, _color.g, _color.b, 1 - (elapsedTime / 1f));
                if (_object.GetComponent<Renderer>() != null)
                    _object.GetComponent<Renderer>().material.color = _color;
                else if (_object.GetComponent<SpriteRenderer>() != null)
                    _object.GetComponent<SpriteRenderer>().material.color = _color;
                else if (_object.GetComponent<Image>() != null)
                    _object.GetComponent<Image>().material.color = _color;

                yield return null;
                }
    }
    #endregion
}
