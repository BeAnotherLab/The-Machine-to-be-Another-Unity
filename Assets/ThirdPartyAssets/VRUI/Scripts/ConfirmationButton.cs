﻿using System;
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

        [SerializeField] private BoolGameEvent _showSelectionRadialEvent;
        [SerializeField] private VRInteractiveItem m_InteractiveItem;       // The interactive item for where the user should click to load the level.

        [SerializeField] private UserStateGameEvent selfStateGameEvent;
        [SerializeField] private UserStateVariable selfState;
        
        public bool gazeOver;                                            // Whether the user is looking at the VRInteractiveItem currently.
         
        private void Awake()
        {
            if (instance == null) instance = this;
        }

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

        public void HandleSelectionComplete() //TODO redundant with Button Logic SelfUserSTateChanged
        {
            if (gazeOver) { //hide TODO use panel dimmer
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<MeshCollider>().enabled = false;
                selfState.Value = UserState.readyToStart;
                selfStateGameEvent.Raise(selfState.Value);
            }
            HandleOut(); //deselect            
        }
        
        private void HandleOver()
        {
            // When the user looks at the rendering of the scene, show the radial.
            if (XRDevice.userPresence == UserPresenceState.Present)
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