using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvertServos : MonoBehaviour
{
   public delegate void OnInvertAxis(bool x, bool y);
   public static OnInvertAxis InvertAxis;
 
   [SerializeField] private Toggle _invertXToggle;
   [SerializeField] private Toggle _invertYToggle;

   public void Invert() {
      InvertAxis(_invertXToggle.isOn, _invertYToggle.isOn);
   }
   
}
