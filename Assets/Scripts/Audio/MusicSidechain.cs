using System;
using UnityEngine;
using DG.Tweening; // <- Make sure to include this

public class MusicSidechain : MonoBehaviour
{
   [SerializeField] private AudioSource _music;
   [SerializeField] private float _lowerVolumeTo;
   
   private void OnEnable()
   {
      AudioManager.PlayingInstruction += LowerVolume;
      AudioManager.FinishedInstruction += IncreaseVolume;
   }

   private void OnDisable()
   {
      AudioManager.PlayingInstruction -= LowerVolume;
      AudioManager.FinishedInstruction -= IncreaseVolume;
   }

   private void LowerVolume()
   {
      _music.DOFade(0.5f, 0.1f);
   }

   private void IncreaseVolume()
   {
      _music.DOFade(1, 0.5f);
   }
}
