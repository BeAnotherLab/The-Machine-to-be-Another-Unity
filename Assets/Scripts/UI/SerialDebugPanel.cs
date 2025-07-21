using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SerialDebugPanel : MonoBehaviour
{
    [SerializeField] private string[] commands;
    [SerializeField] private GameObject _buttons;

    public delegate void OnSendArduinoCommand(string command);
    public static OnSendArduinoCommand SendArduinoCommand;
    
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
                SendArduinoCommand(commands[index]);
            });
            
            i++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
