using ScriptableObjectArchitecture;
using UnityEngine;

public class HeightPresenceDetection : MonoBehaviour
{
    [Header("UserStateVariables")]
    public UserStateVariable previousSelfState;
    public UserStateVariable selfState;
    public UserStateGameEvent selfStateGameEvent;
    
    [Header("References")]
    public Rigidbody headsetRigidbody; // Optional, but helps with velocity checking

    [Header("Height Thresholds (meters)")]
    [Tooltip("Y position below which the headset is considered removed.")]
    [Range(0.2f, 2f)]
    public float removalYThreshold = 0.8f;

    [Tooltip("Y position above which the headset is considered worn again.")]
    [Range(0.2f, 2f)]
    public float wornYThreshold = 1.0f;

    [Header("Velocity Threshold")]
    [Tooltip("Max headset speed (m/s) to consider it stationary.")]
    [Range(0.0f, 1f)]
    public float velocityThreshold = 0.05f;

    [Header("Timing")]
    [Tooltip("How long the headset must stay below threshold before confirming removal.")]
    [Range(0f, 5f)]
    public float requiredDuration = 2.0f;

    private float timer;
    [HideInInspector]
    public float currentHeadY;
    
    private void Update() //Monitor VR headset state changes to infer user presence
    {
        float headY = transform.position.y;
        float speed = 0f;

        if (headsetRigidbody != null)
        {
            speed = headsetRigidbody.linearVelocity.magnitude;
        }

        switch (previousSelfState.Value)
        {
            case UserState.headsetOn: //TODO check enum index 
                if (headY < removalYThreshold && speed < velocityThreshold)
                {
                    timer += Time.deltaTime;
                    if (timer >= requiredDuration)
                    {
                        timer = 0f;
                        previousSelfState.Value = selfState.Value;
                        selfState.Value = UserState.headsetOff; 
                        selfStateGameEvent.Raise(UserState.headsetOff);
                    }
                }
                else timer = 0f;
                break;

            case UserState.headsetOff:
                if (headY > wornYThreshold)
                {
                    previousSelfState.Value = selfState.Value;
                    selfState.Value = UserState.headsetOn;
                    selfStateGameEvent.Raise(UserState.headsetOn);
                }
                break;
        }

        currentHeadY = transform.position.y;
    }
    
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;

        // Removal Threshold
        Gizmos.color = Color.red;
        DrawThresholdGizmo(center, removalYThreshold, "Removal Y Threshold");

        // Worn Threshold
        Gizmos.color = Color.green;
        DrawThresholdGizmo(center, wornYThreshold, "Worn Y Threshold");
    }

    private void DrawThresholdGizmo(Vector3 basePos, float yThreshold, string label)
    {
        Vector3 pos = new Vector3(basePos.x, yThreshold, basePos.z);

        // Draw cross lines
        float size = 0.3f;
        Gizmos.DrawLine(pos + Vector3.left * size, pos + Vector3.right * size);
        Gizmos.DrawLine(pos + Vector3.forward * size, pos + Vector3.back * size);
        Gizmos.DrawWireSphere(pos, 0.025f);

#if UNITY_EDITOR
        // Draw label above
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(pos + Vector3.up * 0.05f, label);
#endif
    }
}
