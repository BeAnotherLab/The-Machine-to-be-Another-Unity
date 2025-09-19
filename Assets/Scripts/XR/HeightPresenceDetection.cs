using ScriptableObjectArchitecture;
using UnityEngine;

public class HeightPresenceDetection : MonoBehaviour
{
    public delegate void OnHeadsetEnterVolume();
    public static OnHeadsetEnterVolume HeadsetEnteredArea;
    
    public delegate void OnHeadsetExitVolume();
    public static OnHeadsetExitVolume HeadsetExitedArea;

    [Tooltip("The 3D collider volume that represents the detection boundary.")]
    [SerializeField] private Collider _detectionVolume;
    
    private bool _wasInsideLastFrame;
    
    private void Update()
    {
        // Is the headset (this.transform) currently within the volume?
        bool isInside = _detectionVolume.bounds.Contains(transform.position);

        // If state has changed, trigger events
        if (!_wasInsideLastFrame && isInside) HeadsetEnteredArea();
        else if (_wasInsideLastFrame && !isInside) HeadsetExitedArea();

        _wasInsideLastFrame = isInside;
    }

}
