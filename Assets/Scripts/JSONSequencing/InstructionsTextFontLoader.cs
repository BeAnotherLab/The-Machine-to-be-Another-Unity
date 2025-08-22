using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class FontColorLoader : MonoBehaviour
{
    private string _fontColorFile = "font_color.txt";

    private void Start()
    {
        var text = GetComponent<Text>();
        if (text == null)
        {
            Debug.LogError("No UnityEngine.UI.Text component found on this GameObject.");
            return;
        }

        string path = ContentPath.Static(_fontColorFile);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Font color file not found: {path}");
            return;
        }

        string hex = File.ReadAllText(path).Trim();
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            text.color = color;
        }
        else
        {
            Debug.LogWarning($"Invalid color code in file: {hex}");
        }
    }
}