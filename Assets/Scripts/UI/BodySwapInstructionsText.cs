using System;
using UnityEngine;

public class BodySwapInstructionsText : MonoBehaviour //TODO inherit Instructions text?  
{
    [SerializeField] private GameObject _instructionsImages;

    private void OnEnable()
    {
        JsonSequenceController.HidePanel += Hide;
        JsonSequenceController.ShowPanel += Show;
    }

    private void OnDisable()
    {
        JsonSequenceController.HidePanel -= Hide;
        JsonSequenceController.ShowPanel -= Show;
    }

    private void Start()
    {
        _instructionsImages.GetComponent<PanelDimmer>().Hide();
    }

    public void FadeInImages() //called by timeline
    {
        _instructionsImages.GetComponent<PanelDimmer>().Show();
    }

    public void FadeOutImages() //called by timeline
    {
        _instructionsImages.GetComponent<PanelDimmer>().Hide();
    }
    
    public void OtherUserStateChanged(UserState otherUserState)
    {
        if (otherUserState == UserState.headsetOff)
        {
            _instructionsImages.GetComponent<PanelDimmer>().Hide();
        }
    }

    private void Hide()
    {
        GetComponent<PanelDimmer>().Hide();
    }

    private void Show()
    {
        GetComponent<PanelDimmer>().Show();
    }
}
