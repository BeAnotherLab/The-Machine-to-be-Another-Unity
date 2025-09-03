using UnityEngine;
using DG.Tweening;

public class ConfirmationButtonGraphics : MonoBehaviour
{
    [Header("Materials")] 
    public Material buttonOff; //TODO make private
    public Material buttonOn; //TODO make private

    [Header("Scale Settings")]
    [SerializeField] private float _scaleAmount;
    [SerializeField] private float _delay; // Time between pulses
    [SerializeField] private float _gazeOnMultiplier;

    private Tween _scaleTween;
    private bool _buttonIsOn;
    private float _baseYScale;
    private MeshRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        _baseYScale = transform.localScale.y; // assuming Y is the base
    }

    private void OnEnable()
    {
        if (!_buttonIsOn) StartIdleAnimation();
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
    }

    public void SwitchSelection(bool on)
    {
        _buttonIsOn = on;

        _scaleTween?.Kill(); // Stop previous animation

        if (on)
        {
            _renderer.material = buttonOn;

            float targetScale = _baseYScale * _scaleAmount * _gazeOnMultiplier;
            Vector3 targetVec = new Vector3(2.8f * targetScale, targetScale, targetScale);

            _scaleTween = transform
                .DOScale(targetVec, 0.4f)
                .SetEase(Ease.OutCubic);
        }
        else
        {
            _renderer.material = buttonOff;
            if (gameObject.activeSelf) StartIdleAnimation();
        }
    }

    private void StartIdleAnimation()
    {
        float minY = _baseYScale;
        float maxY = _baseYScale * _scaleAmount;

        Vector3 minVec = new Vector3(2.8f * minY, minY, minY);
        Vector3 maxVec = new Vector3(2.8f * maxY, maxY, maxY);

        _scaleTween = transform
            .DOScale(maxVec, _delay)
            .SetEase(Ease.OutCubic)
            .SetLoops(-1, LoopType.Yoyo)
            .From(minVec);
    }
}
