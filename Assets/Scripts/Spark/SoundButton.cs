using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundButton : MonoBehaviour
{ 
    
   [SerializeField] private AudioSource _syncSound;
   [SerializeField] private AudioSource _backgroundSound;
   [SerializeField] private Button _syncButton;

    private void Awake()
    {
        _syncButton.onClick.AddListener(delegate { StartCoroutine(SyncSound()); });        
    }
    private IEnumerator SyncSound()
    {
        _backgroundSound.Pause();
        _syncSound.Play();
        yield return new WaitForSeconds(0.5f);
        _backgroundSound.Play();
    }
}
