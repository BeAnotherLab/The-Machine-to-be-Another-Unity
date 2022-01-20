using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VRStandardAssets.Utils;

public class QuestionnaireVRToggle : MonoBehaviour
{
    [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.

    [SerializeField] private Toggle _toggle;
    [SerializeField] private GameEvent _showSelectionRadialEvent;
    [SerializeField] private GameEvent _hideSelectionRadialEvent;
    
    private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.

    private void OnEnable()
    {
        m_InteractiveItem.OnOver += HandleOver;
        m_InteractiveItem.OnOut += HandleOut;
    }

    private void OnDisable()
    {
        m_InteractiveItem.OnOver -= HandleOver;
        m_InteractiveItem.OnOut -= HandleOut;
    }

    private void HandleOver()
    {
        // When the user looks at the rendering of the scene, show the radial.
        if (XRDevice.userPresence == UserPresenceState.Present)
        {
            _showSelectionRadialEvent.Raise();
            LeanTween.scale(gameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.45f).setEaseOutBounce();
            //LeanTween.color(gameObject, Color.white, 0.25f).setEaseOutCubic();
            m_GazeOver = true;
        }
    }

    private void HandleOut()
    {
        // When the user looks away from the rendering of the scene, hide the radial.
        _hideSelectionRadialEvent.Raise();
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.45f).setEaseOutBounce();
        //LeanTween.color(gameObject, Color.gray, 0.25f).setEaseOutCubic();
        m_GazeOver = false;
    }

    public void HandleSelectionComplete()
    {
        if (m_GazeOver)
            _toggle.isOn = !_toggle.isOn;
        HandleOut();            
    }
}
