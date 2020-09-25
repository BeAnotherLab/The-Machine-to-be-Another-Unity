using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{

    public Material buttonOff, buttonOn;
    public static ConfirmationButtonGraphics instance;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        StartCoroutine(AnimateButton());
    }


    public void SwitchSelection(bool onOff) {
        if (onOff) {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOn;
            StopCoroutine(AnimateButton());
            }

        else {
            this.gameObject.GetComponent<MeshRenderer>().material = buttonOff;
            StartCoroutine(AnimateButton());
        }
    }

    private IEnumerator AnimateButton() {
        yield return new WaitForSeconds(0.6f);
        LeanTween.scale(gameObject, new Vector3(1.1f, 1.1f, 1.1f), 0.5f).setEaseOutBounce();

        yield return new WaitForSeconds(0.6f);
        
        LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), 0.5f).setEaseOutBounce();
        StartCoroutine(AnimateButton());
    }
}
