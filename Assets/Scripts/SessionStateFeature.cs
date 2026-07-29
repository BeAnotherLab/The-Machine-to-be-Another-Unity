using UnityEngine;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.NativeTypes;
using UnityEngine.XR.OpenXR;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
using UnityEditor.Build;
#endif

#if UNITY_EDITOR
[OpenXRFeature(UiName = "Session State Tracker",
    BuildTargetGroups = new[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android },
    Company = "BeAnotherLab",
    Desc = "Tracks XrSessionState to determine user presence.",
    FeatureId = "com.beanotherlab.presence",
    Version = "1.0.0")]
#endif

[CreateAssetMenu(fileName = "SessionStateFeature", menuName = "SessionStateFeature")]

public class SessionStateFeature : OpenXRFeature
{
    public delegate void OnUserPresent();
    public static OnUserPresent UserPresent;

    public delegate void OnUserLeft();
    public static OnUserLeft UserLeft;

    private static int _currentState;
    
    protected override void OnSessionStateChange(int oldState, int newState)
    {
        base.OnSessionStateChange(oldState, newState);
        _currentState = newState;
        //if (newState == (int) XrSessionState.Focused) UserPresent();
        //else if (newState == (int)XrSessionState.Idle) UserLeft();
        //Debug.Log($"[SessionStateFeature] Session state changed: {((XrSessionState) oldState).ToString()} → { ((XrSessionState) newState).ToString()}");
    }
    
    public static int GetCurrentState()
    {
        return _currentState;
    }
}