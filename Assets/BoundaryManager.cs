using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    [SerializeField] private GridAlphaController gridAlphaController;

    private void OnEnable()
    {
        HeightPresenceDetection.HeadsetEnteredArea += HandleHeadsetEntered;
        HeightPresenceDetection.HeadsetExitedArea += HandleHeadsetExited;
    }

    private void OnDisable()
    {
        HeightPresenceDetection.HeadsetEnteredArea -= HandleHeadsetEntered;
        HeightPresenceDetection.HeadsetExitedArea -= HandleHeadsetExited;
    }

    private void HandleHeadsetEntered()
    {
        // Headset inside boundary - fade grid in (alpha 1)
        gridAlphaController.FadeToAlpha(0f);
    }

    private void HandleHeadsetExited()
    {
        // Headset outside boundary - fade grid out (alpha 0)
        gridAlphaController.FadeToAlpha(1f);
    }
}