using System.IO;
using UnityEngine;

public class ConfirmationButtonGraphicsLoader : MonoBehaviour
{
    private void Start()
    {
        var buttonGraphics = GetComponent<ConfirmationButtonGraphics>();
        if (buttonGraphics == null)
        {
            Debug.LogError("ConfirmationButtonGraphics component not found on object.");
            return;
        }

        LoadTextureToMaterial(buttonGraphics.buttonOff, "start_button_off.png");
        LoadTextureToMaterial(buttonGraphics.buttonOn, "start_button_on.png");
    }

    private void LoadTextureToMaterial(Material mat, string filename)
    {
        if (mat == null)
        {
            Debug.LogWarning("Material is null, cannot apply texture.");
            return;
        }

        string path = ContentPath.Image(filename);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Texture file not found: {path}");
            return;
        }

        byte[] data = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(data);
        texture.Apply();

        mat.mainTexture = texture;
    }
}