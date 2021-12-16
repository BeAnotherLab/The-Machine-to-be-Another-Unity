using System;
using System.Collections;
using ScriptableObjectArchitecture;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRStandardAssets.Utils;
using UnityEngine.XR;

namespace VRStandardAssets.Menu
{
    public class ConfirmationButton : MonoBehaviour
    {
        public static ConfirmationButton instance;

        public event Action<ConfirmationButton> OnButtonSelected;           // This event is triggered when the selection of the button has finished.

        [SerializeField] private CustomSelectionRadial m_SelectionRadial;         // This controls when the selection is complete.
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.

        [SerializeField] private UserStateGameEvent selfStateGameEvent;
        [SerializeField] private UserStateVariable selfState;

        private bool m_GazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.
        
        public void SelfUserStateChanged(UserState selfUserState)
        {
            if (selfUserState == UserState.readyToStart)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<MeshCollider>().enabled = false;    
            } else if (selfUserState == UserState.headsetOff)
            {
                GetComponent<MeshRenderer>().enabled = true;
                GetComponent<MeshCollider>().enabled = true;
            }
            }
        
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

        private void HandleSelectionComplete()
        {
            if (m_GazeOver) {
                selfState.Value = UserState.readyToStart;
                selfStateGameEvent.Raise(selfState.Value);
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<MeshCollider>().enabled = false;
            }
            HandleOut();            
        }

    }
}