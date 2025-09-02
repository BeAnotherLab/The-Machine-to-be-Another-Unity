using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using UnityEngine.XR;
using ScriptableObjectArchitecture;
using UnityEngine.Serialization;

namespace VRStandardAssets.Menu //TODO move to own namespace?
{
    public class LanguageButton : MonoBehaviour //Make inherit from ConfirmationButton class
    {
        [SerializeField] private Vector3 _scaleOut; 
        [SerializeField] private Vector3 _scaleOn;
        
        [SerializeField] private BoolGameEvent _showSelectionRadialEvent;
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.
        [SerializeField] private StringGameEvent _languageChangeEvent;
        [SerializeField] private string _language;
        [SerializeField] private UserStateVariable _selfState;

        private bool m_GazeOver;   // Whether the user is looking at the VRInteractiveItem currently.

        private void OnEnable()
        {
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
            CustomSelectionRadial.SelectionComplete += HandleSelectionComplete;
        }

        private void OnDisable()
        {
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
            CustomSelectionRadial.SelectionComplete -= HandleSelectionComplete;
        }
        
        public void HandleSelectionComplete()
        {
            if (m_GazeOver) _languageChangeEvent.Raise(_language);
            HandleOut();            
        }
        
        private void HandleOver()
        {
            if (_selfState.Value == UserState.headsetOn)
            {
                _showSelectionRadialEvent.Raise(true);
                LeanTween.scale(gameObject, _scaleOn, 0.45f).setEaseOutBounce();
                LeanTween.color(gameObject, Color.white, 0.25f).setEaseOutCubic();
                m_GazeOver = true;
            }
        }

        private void HandleOut()
        {
            _showSelectionRadialEvent.Raise(false);
            LeanTween.scale(gameObject, _scaleOut, 0.45f).setEaseOutBounce();
            LeanTween.color(gameObject, Color.gray, 0.25f).setEaseOutCubic();
            m_GazeOver = false;
        }

    }
}