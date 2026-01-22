using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using UnityEngine.XR;
using ScriptableObjectArchitecture;
using DG.Tweening;

namespace VRStandardAssets.Menu
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
        private Tween _scaleTween;
        private Tween _colorTween;
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

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

        public void SetLanguage(string language)
        {
            _language = language;
        }
        
        public void HandleSelectionComplete()
        {
            if (m_GazeOver) _languageChangeEvent.Raise(_language);
            HandleOut();            
        }

        private void HandleOver()
        {
            if (_selfState.Value != UserState.headsetOn) return;

            _showSelectionRadialEvent.Raise(true);
            m_GazeOver = true;

            // Kill any ongoing tweens to avoid conflicts
            _scaleTween?.Kill();
            _colorTween?.Kill();

            // Start DOTween animations
            _scaleTween = transform.DOScale(_scaleOn, 0.45f).SetEase(Ease.OutCubic);
            if (_renderer != null)
            {
                _colorTween = _renderer.material
                    .DOColor(Color.white, 0.25f)
                    .SetEase(Ease.OutCubic);
            }
        }

        private void HandleOut()
        {
            _showSelectionRadialEvent.Raise(false);
            m_GazeOver = false;

            // Kill any ongoing tweens to avoid conflicts
            _scaleTween?.Kill();
            _colorTween?.Kill();

            // Start DOTween animations
            _scaleTween = transform.DOScale(_scaleOut, 0.45f).SetEase(Ease.OutCubic);
            if (_renderer != null)
            {
                _colorTween = _renderer.material
                    .DOColor(Color.gray, 0.25f)
                    .SetEase(Ease.OutCubic);
            }
        }
    }
}
