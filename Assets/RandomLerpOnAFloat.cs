using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLerpOnAFloat : MonoBehaviour
{

    public float minimum = 15f;
    public float maximum = 165f;

    private float currentTarget = 90f;
    private float speed = 0.5f;
    private float previousTarget = 15f;

    [HideInInspector]
    public float t = 90f;

    public float ChangingValue()
    {

        if (currentTarget < previousTarget) {
            t -= speed * Time.deltaTime;

            if (t <= currentTarget)
                CreateNewTarget();
        }

        else {
            t += speed * Time.deltaTime;

            if (t >= currentTarget)
                CreateNewTarget();
        }

        return t;
    }

    private void CreateNewTarget()
    {
        previousTarget = currentTarget;
        currentTarget = Random.Range(minimum, maximum);
        speed = Random.Range(0.25f, 0.65f) * 50;
    }


}
