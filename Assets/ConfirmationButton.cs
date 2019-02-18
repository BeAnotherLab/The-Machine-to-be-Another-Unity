using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using UnityEngine.XR;

namespace VRStandardAssets.Menu
{
    // This script is for loading scenes from the main menu.
    // Each 'button' will be a rendering showing the scene
    // that will be loaded and use the SelectionRadial.
    public class ConfirmationButton : MonoBehaviour
    {
        public static ConfirmationButton instance;

        public event Action<ConfirmationButton> OnButtonSelected;           // This event is triggered when the selection of the button has finished.

        [SerializeField] private CustomSelectionRadial m_SelectionRadial;         // This controls when the selection is complete.
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.

        private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.

        private void Awake()
        {
            if (instance == null) instance = this;
        }

        private void OnEnable()
        {
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
            m_SelectionRadial.OnSelectionComplete += HandleSelectionComplete;
        }

        private void OnDisable()
        {
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
            m_SelectionRadial.OnSelectionComplete -= HandleSelectionComplete;
        }

        private void HandleOver()
        {
            // When the user looks at the rendering of the scene, show the radial.
            if (XRDevice.userPresence == UserPresenceState.Present)
            {
                m_SelectionRadial.Show();
                LeanTween.scale(gameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.45f).setEaseOutBounce();
                LeanTween.color(gameObject, Color.white, 0.25f).setEaseOutCubic();
                m_GazeOver = true;
            }
        }

        private void HandleOut()
        {
            // When the user looks away from the rendering of the scene, hide the radial.
            m_SelectionRadial.Hide();
            LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.45f).setEaseOutBounce();
            LeanTween.color(gameObject, Color.gray, 0.25f).setEaseOutCubic();

            m_GazeOver = false;
        }

        private void HandleSelectionComplete()
        {
            //the user is ready
            StatusManager.instance.ThisUserIsReady();
        }

    }
}