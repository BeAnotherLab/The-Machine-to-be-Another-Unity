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
        //parse configuration string
        int selfSpheres = (int) Char.GetNumericValue(configuration.ToCharArray()[1]); //the number of spheres self sees
        int otherSpheres = (int) Char.GetNumericValue(configuration.ToCharArray()[4]); //the number of spheres the other sees
        char directionFacing = configuration.ToCharArray()[7]; //the direction the other is facing
        Transform visibleSide; //the side visible to the other
        Transform oppositeSide; //the side not visible to the other

        if (directionFacing == 'L') //if we want the other facing left
        {
            visibleSide = _leftSpheres; 
            oppositeSide = _rightSpheres;
        }
        else //if we want the other facing right
        {
            visibleSide = _rightSpheres;
            oppositeSide = _leftSpheres;
        }
            
        DisplaySpheresForSelfOrOther(otherSpheres, visibleSide); //display the spheres for the other
        DisplaySpheresForSelfOrOther(selfSpheres - otherSpheres, oppositeSide); //display the spheres for oneself
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
