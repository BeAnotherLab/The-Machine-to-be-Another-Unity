using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialDebugPanel : MonoBehaviour
{
    [SerializeField] private string[] commands;
    [SerializeField] private GameObject _buttons;

    // Start is called before the first frame update
    private void Start()
    {
        int i = 0;
        foreach (Button button in _buttons.GetComponentsInChildren<Button>())
        {
            var index = i;
            
            button.gameObject.GetComponentInChildren<Text>().text = commands[i];
            button.onClick.AddListener(delegate
            {
                ArduinoManager.instance.SendCommand(commands[index]);
            });
            
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
