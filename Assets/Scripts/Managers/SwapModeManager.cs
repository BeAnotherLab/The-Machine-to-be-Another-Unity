using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwapModes {AUTO_SWAP, MANUAL_SWAP, CURTAIN_MANUAL_SWAP};

public class SwapModeManager : MonoBehaviour
{
    public static SwapModeManager instance;

    public SwapModes swapMode;

    public delegate void OnSwapModeChanged(SwapModes swapmodes);
    public static OnSwapModeChanged SwapModeChanged = delegate(SwapModes modes) {  };
    
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SwapModeChanged(swapMode);
    }

    public void SetSwapModes(SwapModes mode) //TODO get rid of all singleton stuff
    { //TODO get rid of swap mode manager completely?
        
        switch (mode)
        {
            
            case SwapModes.AUTO_SWAP:
                
                StatusManager.instance.Standby(true, true); //go to initial state

                break;

            case SwapModes.MANUAL_SWAP:
                
                StatusManager.instance.Standby(true, false); //go to initial state

                break;
                
            case SwapModes.CURTAIN_MANUAL_SWAP:
                
                StatusManager.instance.Standby(true, false); //go to initial state
                break;
        }

        swapMode = mode;
    }
    
}
