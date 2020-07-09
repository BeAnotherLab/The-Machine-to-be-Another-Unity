using System;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class CustomPlayer : NetworkBehaviour
    {
        private GameObject _mainCamera;
        
        private void Awake()
        {
            _mainCamera = GameObject.Find("Main Camera");
        }

        // need to use FixedUpdate for rigidbody
        void FixedUpdate()
        {
            // only let the local player control the racket.
            // don't control other player's rackets
            if (isLocalPlayer)
                transform.rotation = _mainCamera.transform.rotation;
        }
    }
}