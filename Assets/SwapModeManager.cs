using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapModeManager : MonoBehaviour
{

    public static SwapModeManager instance;

    public enum SwapModes {AUTO_SWAP, MANUAL_SWAP, SERVO_SWAP};

    public SwapModes swapMode;


    private void Awake()
    {
        if (instance == null) instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSwapMode(SwapModes mode)
    {
        switch (mode)
        {
            case SwapModes.AUTO_SWAP:
                //deactivate servos
                //hide serial port dropdown, show repeater toggle
                //move video with other pose
                //enable status management
                FindObjectOfType<StatusManager>().statusManagementOn = true;
                //auto play instructions audio when both ready. reset timer                
                break;
            case SwapModes.MANUAL_SWAP:
                //deactivate servos
                //hide serial port dropdown, show repeater toggle
                //move video with other pose
                //enable status management
                FindObjectOfType<StatusManager>().statusManagementOn = true;
                //stop auto swap instructions audio
                break;
            case SwapModes.SERVO_SWAP:
                //enable servos
                //hide repeater toggle, show serial dropdown
                //keep video in front of camera
                //disable status management
                FindObjectOfType<StatusManager>().statusManagementOn = false;
                //hide repeater toggle
                break;
        }

        swapMode = mode;
    }
}
