using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class VisualPlayer : MonoBehaviour
{
    [SerializeField] private Image _imageRenderer;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private PanelDimmer _panelDimmer;
    
    [SerializeField] private string _fileName; //TODO for debugging

    private Texture2D _imageTexture;
    
    private void Start()
    {
        _panelDimmer.Hide();
    }

    private void OnEnable()
    {
        JsonSequenceController.ShowVisual += Show;
        JsonSequenceController.HideVisual += Hide;
    }

    private void OnDisable()
    {
        JsonSequenceController.ShowVisual -= Show;
        JsonSequenceController.HideVisual -= Hide;
    }

    public void Show(string filename)
    {
        _fileName = filename;
        string ext = Path.GetExtension(filename).ToLower();
        if (ext == ".mp4") ShowVideo(filename);
        else if (ext == ".png") ShowImage(filename);
    }
    
    private void ShowImage(string filename)
    {
        _videoPlayer.Stop();
        _videoPlayer.gameObject.SetActive(false);
        _imageRenderer.gameObject.SetActive(true);

        string path = ContentPath.Image(filename);
        byte[] fileData = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(fileData);
        tex.Apply();

        // Convert to sprite
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), // pivot in center
            100f                     // pixels per unit (adjust if needed)
        );

        _imageRenderer.sprite = sprite;
        _panelDimmer.Show();
    }

    private void ShowVideo(string filename)//TODO manage loading times. load previously into sequence or wait here?
    {
        _imageRenderer.gameObject.SetActive(false);
        _videoPlayer.gameObject.SetActive(true);

        string path = ContentPath.Video(filename);

        _videoPlayer.url = path;
        _videoPlayer.Play();
        _panelDimmer.Show();
    }
    

    private void Hide()
    {       
        _panelDimmer.Hide();
        _videoPlayer.Stop();
       // StartCoroutine(WaitAndDisableVisual());
    }
    
    private IEnumerator WaitAndDisableVisual() //TODO this creates issues when puttin one image after the other too fast
    {
        yield return new WaitForSeconds(2f);
        _imageRenderer.gameObject.SetActive(false);
        _videoPlayer.gameObject.SetActive(false);
    }

}
