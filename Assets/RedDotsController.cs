using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedDotsController : MonoBehaviour
{
    public static RedDotsController instance;

    [SerializeField] private Transform _leftSpheres;
    [SerializeField] private Transform _rightSpheres;
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void Show(string configuration)
    {
        int selfSpheres = (int) Char.GetNumericValue(configuration.ToCharArray()[1]);
        int otherSpheres = (int) Char.GetNumericValue(configuration.ToCharArray()[4]);
        char directionFacing = configuration.ToCharArray()[7];
        Transform visibleSide, otherSide;

        if (directionFacing == 'L')
        {
            visibleSide = _leftSpheres;
            otherSide = _rightSpheres;
        }
        else
        {
            visibleSide = _rightSpheres;
            otherSide = _leftSpheres;
        }
            
        DisplaySpheresForSelfOrOther(otherSpheres, otherSide);
        DisplaySpheresForSelfOrOther(selfSpheres - otherSpheres, visibleSide);
    }

    private void DisplaySpheresOnSide(Transform side, bool top, bool center, bool bottom)
    {
        side.GetComponentsInChildren<MeshRenderer>()[0].enabled = top;
        side.GetComponentsInChildren<MeshRenderer>()[1].enabled = center;
        side.GetComponentsInChildren<MeshRenderer>()[2].enabled = bottom;
    }

    private void DisplaySpheresForSelfOrOther(int numberSpheres, Transform side)
    {
        if(numberSpheres == 0) DisplaySpheresOnSide(side, false, false, false);
        if (numberSpheres == 1) DisplaySpheresOnSide(side, false, true, false);
        else if (numberSpheres == 2) DisplaySpheresOnSide(side, true, false, true);
        else if (numberSpheres == 3) DisplaySpheresOnSide(side, true, true, true);
    }
    
}
