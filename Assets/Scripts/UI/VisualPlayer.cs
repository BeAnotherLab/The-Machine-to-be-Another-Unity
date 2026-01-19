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
    
    [SerializeField] private string _fileName; //for debugging
    [SerializeField] private string _fileType; //for debugging

    private Texture2D _imageTexture;
    private RawImage _videoPlayerRawImage;

    private void Start()
    {
        _panelDimmer.Hide();
        _videoPlayerRawImage = _videoPlayer.gameObject.GetComponent<RawImage>();
    }

    private void OnEnable()
    {
        JsonSequenceController.LoadVisual += Load;
        JsonSequenceController.ShowVisual += Show;
        JsonSequenceController.HideVisual += Hide;
    }

    private void OnDisable()
    {
        JsonSequenceController.LoadVisual -= Load;
        JsonSequenceController.ShowVisual -= Show;
        JsonSequenceController.HideVisual -= Hide;
    }

    private void Load(string filename)
    {
        _fileName = filename;
        string ext = Path.GetExtension(filename).ToLower();
        if (ext == ".mp4") LoadVideo(filename);
        else if (ext == ".png") LoadImage(filename);
    }

    private void Show()
    {
        if (_fileType == "Video") ShowVideo();
        else if (_fileType == "Image") ShowImage();
    }
    
    private void ShowImage()
    {
        _videoPlayer.Stop();
        _videoPlayerRawImage.enabled = false;
        _imageRenderer.gameObject.SetActive(true);

        _panelDimmer.Show();
    }

    private void LoadImage(string filename)
    {
        _fileType = "Image";
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
    }
    
    private void ShowVideo()//TODO manage loading times. load previously into sequence or wait here?
    {
        _imageRenderer.gameObject.SetActive(false);
        _videoPlayerRawImage.enabled = true;
        _videoPlayer.Play();
        _panelDimmer.Show();
    }

    private void LoadVideo(string filename)
    {
        _fileType = "Video";
        string path = ContentPath.Video(filename);
        _videoPlayer.url = path;
        _videoPlayer.Play();
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
        _videoPlayerRawImage.enabled = false;
    }

}
