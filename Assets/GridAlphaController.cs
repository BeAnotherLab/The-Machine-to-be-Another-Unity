using UnityEngine;
using DG.Tweening;

public class GridAlphaController : MonoBehaviour
{
    [SerializeField] private Material gridMaterial;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float defaultLineWidth = 0.05f;

    private Tween _lineWidthTween;

    public float DefaultLineWidth => defaultLineWidth;

    public void TweenLineWidth(float targetLineWidth)
    {
        if (gridMaterial == null) return;

        _lineWidthTween?.Kill();

        float currentLineWidth = gridMaterial.GetFloat("_LineWidth");

        _lineWidthTween = DOTween.To(
            () => currentLineWidth,
            value =>
            {
                currentLineWidth = value;
                gridMaterial.SetFloat("_LineWidth", currentLineWidth);
            },
            targetLineWidth,
            fadeDuration
        ).SetEase(Ease.OutCirc);
    }
}