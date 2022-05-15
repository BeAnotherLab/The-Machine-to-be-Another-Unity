using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using VRStandardAssets.Utils;

public class QuestionnaireVRSlider : MonoBehaviour
{
    [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.
    [SerializeField] private BoolGameEvent _showSelectionRadialEvent;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    private Scrollbar _scrollbar;
    private GameObject _sliderHandle;
    private Transform reticlePosition;

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

    private void Awake()
    {
        _sliderHandle = GetComponent<Scrollbar>().handleRect.gameObject;
        _scrollbar = GetComponent<Scrollbar>();
        _canvasGroup = transform.parent.transform.parent.GetComponent<CanvasGroup>();
        reticlePosition = Camera.main.gameObject.GetComponent<Reticle>().ReticleTransform;
    }

    private void HandleOver()
    {
        Debug.Log("handle over");
        // When the user looks at the rendering of the scene, show the radial.
        if (XRDevice.userPresence == UserPresenceState.Present && _canvasGroup.alpha == 1)
        {
            _showSelectionRadialEvent.Raise(true);
            m_GazeOver = true;
        }
    }

    private void HandleOut()
    {
        // When the user looks away from the rendering of the scene, hide the radial.
        _showSelectionRadialEvent.Raise(false);
        m_GazeOver = false;
    }

    public void HandleSelectionComplete()
    {
        if (m_GazeOver) {
            float scrollBarSize = GetComponent<RectTransform>().rect.width;
            Vector3 relativeToCanvas = GetComponentInParent<Canvas>().gameObject.transform.InverseTransformPoint(reticlePosition.transform.position);
            float mappedPosition =(relativeToCanvas.x/(scrollBarSize))+0.5f;       
            _sliderHandle.SetActive(true);
            _scrollbar.value = mappedPosition;
        }
        HandleOut();            
    }
}
