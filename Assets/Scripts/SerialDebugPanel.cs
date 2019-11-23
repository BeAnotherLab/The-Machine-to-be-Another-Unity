using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialDebugPanel : MonoBehaviour
{
    [SerializeField] string[] commands;

    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        foreach (Button button in GetComponentsInChildren<Button>())
        {
            var index = i;
            
            button.gameObject.GetComponentInChildren<Text>().text = commands[i];
            button.onClick.AddListener(delegate
            {
                ArduinoControl.instance.SendCommand(commands[index]);
            });
            
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
