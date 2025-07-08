using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uduino;
using UnityEngine.UI;


public class EncoderLibrary : MonoBehaviour {

    public Slider slider;
	void Start () {
        UduinoManager.Instance.alwaysRead = true;
        UduinoManager.Instance.SetReadCallback(ReadEncoder);
    }

    // Reading Encoder thanks to read callback
    void ReadEncoder (string data) {
        slider.value = int.Parse(data);
    }
}
