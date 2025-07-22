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
        [SerializeField] private BoolGameEvent _showSelectionRadialEvent;
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.

        [SerializeField] private UserStateGameEvent selfStateGameEvent;//TODO prepend "_" in name
        [SerializeField] private UserStateVariable selfState; //TODO prepend "_" in name
        
        public bool gazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.

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
            if (gazeOver) { //hide TODO use panel dimmer
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<MeshCollider>().enabled = false;
                selfState.Value = UserState.readyToStart;
                selfStateGameEvent.Raise(selfState.Value);
            }
            HandleOut(); //deselect            
        }
        
        private void HandleOver() //TODO test
        {
            // When the user looks at the rendering of the scene, show the radial.
            if (selfState.Value == UserState.headsetOn)
            {
                _showSelectionRadialEvent.Raise(true);
                gazeOver = true;
                GetComponent<ConfirmationButtonGraphics>().SwitchSelection(gazeOver);
            }
        }

        private void HandleOut()
        {
            // When the user looks away from the rendering of the scene, hide the radial.
            _showSelectionRadialEvent.Raise(false);
            gazeOver = false;     
            GetComponent<ConfirmationButtonGraphics>().SwitchSelection(gazeOver);

        }

        public void OnStandby() //TODO separate logic from underlying VRUI third party package?
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshCollider>().enabled = true;
        }
    }
}