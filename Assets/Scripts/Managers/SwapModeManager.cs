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
    
}
