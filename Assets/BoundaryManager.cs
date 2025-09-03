using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    [SerializeField] private GridAlphaController gridAlphaController;

    private void OnEnable()
    {
        BoundaryTweenTrigger.HeadsetEnteredArea += HandleHeadsetEntered;
        BoundaryTweenTrigger.HeadsetExitedArea += HandleHeadsetExited;
    }

    private void OnDisable()
    {
        BoundaryTweenTrigger.HeadsetEnteredArea -= HandleHeadsetEntered;
        BoundaryTweenTrigger.HeadsetExitedArea -= HandleHeadsetExited;
    }

    private void HandleHeadsetEntered()
    {
        // Headset inside boundary - fade out grid (line width → 0)
        gridAlphaController.TweenLineWidth(0f);
    }

    private void HandleHeadsetExited()
    {
        // Headset outside boundary - fade in grid (line width → default)
        gridAlphaController.TweenLineWidth(gridAlphaController.DefaultLineWidth);
    }
}