using System;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.XR;
using VRStandardAssets.Menu;
using VRStandardAssets.Utils;

public class ConsentConfirmationButton : MonoBehaviour
{
        [SerializeField] private BoolGameEvent _handleSelectionCompleteEvent;
        
        [SerializeField] private CustomSelectionRadial m_SelectionRadial;         // This controls when the selection is complete.
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.
        [SerializeField] private bool _answer;
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

        public void HandleSelectionComplete()
        {
            if (m_GazeOver)
            {
                _handleSelectionCompleteEvent.Raise(_answer);
            }
            HandleOut();            
        }
        
        private void HandleOver()
        {
            // When the user looks at the rendering of the scene, show the radial.
            if (XRDevice.userPresence == UserPresenceState.Present)
            {
                m_SelectionRadial.Show();
                //LeanTween.scale(gameObject, new Vector3(1.2f, 1.2f, 1.2f), 0.45f).setEaseOutBounce();
                //LeanTween.color(gameObject, Color.white, 0.25f).setEaseOutCubic();

                m_GazeOver = true;
                GetComponent<ConfirmationButtonGraphics>().SwitchSelection(m_GazeOver);
            }
        }

        private void HandleOut()
        {
            // When the user looks away from the rendering of the scene, hide the radial.
            m_SelectionRadial.Hide();
            //LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.45f).setEaseOutBounce();
            //LeanTween.color(gameObject, Color.gray, 0.25f).setEaseOutCubic();

            m_GazeOver = false;
            GetComponent<ConfirmationButtonGraphics>().SwitchSelection(m_GazeOver);
        }
    
}



