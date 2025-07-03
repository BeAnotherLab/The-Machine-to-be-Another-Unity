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
    BuildTargetGroups = new[] { BuildTargetGroup.Standalone },
    Company = "BeAnotherLab",
    Desc = "Tracks XrSessionState to determine user presence.",
    FeatureId = "com.beanotherlab.presence",
    OpenxrExtensionStrings = "XR_test",
    Version = "1.0.0")]
#endif

[CreateAssetMenu(fileName = "SessionStateFeature", menuName = "SessionStateFeature")]

public class SessionStateFeature : OpenXRFeature
{
    private static int currentState;

    protected override void OnSessionStateChange(int oldState, int newState)
    {
        base.OnSessionStateChange(oldState, newState);
        currentState = newState;
        Debug.Log($"[SessionStateFeature] Session state changed: {oldState} → {newState}");
    }

    public static bool IsUserPresent()
    {
        return currentState == (int) XrSessionState.Focused || currentState == (int) XrSessionState.Ready;
    }

    public static int GetCurrentState()
    {
        return currentState;
    }
}