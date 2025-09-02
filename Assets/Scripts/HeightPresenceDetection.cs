using ScriptableObjectArchitecture;
using UnityEngine;

public class HeightPresenceDetection : MonoBehaviour
{

    public delegate void OnHeadsetOverThreshold();
    public static OnHeadsetOverThreshold HeadsetOverThreshold;
    
    public delegate void OnHeadsetUnderThreshold();
    public static OnHeadsetUnderThreshold HeadsetUnderThreshold;

    [Tooltip("Y position below which the headset is considered removed.")] 
    [Range(2.5f, 4f)] [SerializeField]
    private float _yThreshold;

    [Range(2.5f, 4f)] [SerializeField]
    private float _previousHeight;
    
    private void Update() //Monitor VR headset height to infer user presence
    {
        
        Debug.Log(transform.position.y);
        
        if (transform.position.y < _yThreshold && _previousHeight >= _yThreshold)  //we just passed under threshold
        {
            HeadsetUnderThreshold();
        }
        else if  (transform.position.y > _yThreshold && _previousHeight <= _yThreshold) //we just passed over threshold
        {
            HeadsetOverThreshold();
        }
        
        _previousHeight = transform.position.y;
    }
}
