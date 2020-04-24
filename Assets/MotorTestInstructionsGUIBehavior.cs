using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorTestInstructionsGUIBehavior : MonoBehaviour
{

    public static MotorTestInstructionsGUIBehavior instance;
    
    // Start is called before the first frame update
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Init()
    {
        
    }
}
