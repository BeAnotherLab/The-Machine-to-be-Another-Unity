using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfirmationButtonGraphics : MonoBehaviour
{
    public Material buttonOff, buttonOn;
    public static ConfirmationButtonGraphics instance;
    private bool _loopAnimation = true;

    private int _idleTween;
    
    void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        _idleTween = LeanTween
            .scale(gameObject, new Vector3(1.3f, 1.3f, 1.3f), 0.7f)
            .setEaseOutCubic()
            .setLoopType(LeanTweenType.pingPong).id;
    }


    public void SwitchSelection(bool _on) {
        if (_on) {
            GetComponent<MeshRenderer>().material = buttonOn;
            LeanTween.pause(_idleTween);
            LeanTween
                .scale(gameObject, new Vector3(1.3f, 1.3f, 1.3f), 0.7f)
                .setEaseOutCubic();
        }
        else {
            GetComponent<MeshRenderer>().material = buttonOff;
            if(gameObject.activeSelf)
                LeanTween.resume(_idleTween);
        }
    }
}
