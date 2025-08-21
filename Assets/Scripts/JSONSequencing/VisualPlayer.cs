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
    [SerializeField] private string _fileName;
    [SerializeField] private RenderTexture _renderTexture;

    private void Start()
    {
        _panelDimmer.Hide();
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

    public void Load(string filename)
    {
        string ext = Path.GetExtension(filename).ToLower();
        if (ext == ".mp4") ShowVideo(filename);
        else if (ext == "png") ShowImage(filename);
    }

    private void ShowImage(string filename)
    {
        _videoPlayer.Stop();
        //_videoPlayer.gameObject.SetActive(false);
        //_imageRenderer.gameObject.SetActive(true);

        string path = ContentPath.Image(filename);
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
        
        _imageRenderer.material.mainTexture = tex;
    }

    private void ShowVideo(string filename)
    {
        //_imageRenderer.gameObject.SetActive(true);
        //_videoPlayer.gameObject.SetActive(true);

        string path = ContentPath.Video(filename);

        _videoPlayer.targetTexture = _renderTexture;
        _imageRenderer.material.mainTexture = _renderTexture;

        _videoPlayer.url = path;
        _videoPlayer.Play();
    }

    private void Hide()
    {       
        _panelDimmer.Hide();
        _videoPlayer.Stop();
        StartCoroutine(WaitAndDisableVisual());
    }

    private void Show()
    {
        _panelDimmer.Show();
    }

    private IEnumerator WaitAndDisableVisual()
    {
        yield return new WaitForSeconds(2f);
        //_imageRenderer.gameObject.SetActive(false);
        //_videoPlayer.gameObject.SetActive(false);
    }

}
