using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{

    public Material buttonOff, buttonOn;
    public static ConfirmationButtonGraphics instance;
    private bool _loopAnimation = true;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        StartCoroutine(AnimateButton());
    }


    public void SwitchSelection(bool _on) {
        if (_on) {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOn;
            _loopAnimation = false;
            StopCoroutine(AnimateButton());
            //LeanTween.resumeAll();
            LeanTween.scale(gameObject, new Vector3(1.3f, 1.3f, 1.3f), 1f).setEaseOutBounce();
            
            }

        else {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOff;
            StartCoroutine(AnimateButton());
            _loopAnimation = true;
        }
    }

    private IEnumerator AnimateButton() {
        yield return new WaitForSeconds(0.6f);
        LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.5f).setEaseOutBounce();

        yield return new WaitForSeconds(0.6f);
        
        LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), 0.5f).setEaseOutBounce();

        if(_loopAnimation)
                StartCoroutine(AnimateButton());
    }
}
