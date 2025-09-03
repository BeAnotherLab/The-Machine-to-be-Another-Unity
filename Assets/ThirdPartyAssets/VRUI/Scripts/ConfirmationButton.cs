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

        //TODO separate the following to keep confirmation button pure VRUI stuff
        [SerializeField] private UserStateGameEvent _selfStateGameEvent;
        [SerializeField] private UserStateVariable _selfState;
        
        public bool gazeOver; // Whether the user is looking at the VRInteractiveItem currently.

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
            if (gazeOver) { //hide TODO use panel dimmer
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<MeshCollider>().enabled = false;
                _selfState.Value = UserState.readyToStart; //TODO separate
                _selfStateGameEvent.Raise(_selfState.Value); //TODO separate
            }
            HandleOut(); //deselect            
        }
        
        private void HandleOver() //TODO test
        {
            // When the user looks at the rendering of the scene, show the radial.
            if (_selfState.Value == UserState.headsetOn) //TODO separate
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

    }
}