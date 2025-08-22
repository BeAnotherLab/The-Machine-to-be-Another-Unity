using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PanelBackgroundLoader : MonoBehaviour
{
    private string _filename = "panel_background.png";

    private void Start()
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("No Image component found on this GameObject.");
            return;
        }

        string path = ContentPath.Static(_filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Panel background file not found at path: {path}");
            return;
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        if (!tex.LoadImage(fileData))
        {
            Debug.LogError("Failed to load texture from image file.");
            return;
        }

        // Create Sprite from Texture2D
        Sprite sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)); // pivot at center

        image.sprite = sprite;
        image.preserveAspect = true;
    }
}