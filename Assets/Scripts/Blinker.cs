using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinker : MonoBehaviour
{

    public Renderer left, right;
    public float interval, duration;

    private bool prev_right;
    private Color on_color, off_color;

    public static Blinker instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
            instance = this;

        on_color = new Color(255, 255, 255, 128);
        off_color = new Color(0, 0, 0, 0);
    }

    public void SetBlink (bool turnOn) {
        if (turnOn)
            InvokeRepeating("Blink", 0f, interval - duration);

        else
            CancelInvoke("Blink");
    }

    private void Blink(){
        Debug.Log("blink should occur");
        if (prev_right)
        {
            left.material.color = on_color;
            prev_right = false;
        }

        else
        {
            ;
            right.material.color = on_color;
            prev_right = true;
        }

        StartCoroutine(TurnOff());
    }
    private IEnumerator TurnOff() {
        
        yield return new WaitForFixedTime(duration);

        left.material.color = off_color;
        right.material.color = off_color;

    }
}
