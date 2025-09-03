using UnityEngine;
using DG.Tweening;

public class GridAlphaController : MonoBehaviour
{
    [SerializeField] private Material gridMaterial;
    [SerializeField] private float fadeDuration = 0.5f;

    private Color _originalColor;
    private Tween _fadeTween;

    private void Awake()
    {
        if (gridMaterial != null)
        {
            _originalColor = gridMaterial.GetColor("_Color");
        }
    }

    public void FadeToAlpha(float targetAlpha)
    {
        if (gridMaterial == null) return;

        // Kill any existing fade tween to prevent overlap
        _fadeTween?.Kill();

        Color currentColor = gridMaterial.GetColor("_Color");

        // Start a new tween from current color to target alpha
        _fadeTween = DOTween.To(
            () => currentColor.a,
            alpha =>
            {
                currentColor.a = alpha;
                gridMaterial.SetColor("_Color", currentColor);
            },
            targetAlpha,
            fadeDuration
        ).SetEase(Ease.InOutSine);
    }
}