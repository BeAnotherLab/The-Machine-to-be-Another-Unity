using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class VisualPlayer : MonoBehaviour
{
    [SerializeField] private Renderer imageRenderer;
    [SerializeField] private VideoPlayer videoPlayer;

    public void Show(string filename)
    {
      /*  string ext = Path.GetExtension(filename).ToLower();

        if (ext == ".mp4") ShowVideo(filename);
        else ShowImage(filename);
        */
    }

    private void ShowImage(string filename)
    {
    /*    videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);

        string path = Path.Combine(Application.dataPath, "Content/Sequence/Images", filename);
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        imageRenderer.material.mainTexture = tex;
        imageRenderer.gameObject.SetActive(true);*/
    }

    private void ShowVideo(string filename)
    {
        /*imageRenderer.gameObject.SetActive(false);

        string path = Path.Combine(Application.dataPath, "Content/Sequence/Videos", filename);
        videoPlayer.url = path;
        videoPlayer.gameObject.SetActive(true);
        videoPlayer.Play();*/
    }

    public void Hide()
    {
        /*imageRenderer.gameObject.SetActive(false);
        videoPlayer.Stop();
        videoPlayer.gameObject.SetActive(false);*/
    }
}
