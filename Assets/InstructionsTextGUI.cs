using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionsTextGUI : MonoBehaviour
{

    public static InstructionsTextGUI instance;

    [SerializeField] private LocalizedText _text;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void SetText(string key)
    {
        GetComponent<CanvasGroup>().alpha = 1;
        _text.key = key;
    }

    public void Hide()
    {
        GetComponent<CanvasGroup>().alpha = 1;
        _text.key = "";
    }
    
    public void ShowText(string key, int time = 4)
    {
        if (time!=0) StartCoroutine (ShowTextCoroutine(key, time));
    }

    private IEnumerator ShowTextCoroutine(string key, int time)
    {
        SetText(key);
        yield return new WaitForSeconds(time);
        Hide();
    }
}
