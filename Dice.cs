using UnityEngine;
using DG.Tweening;

public class Dice : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _sprites = new Sprite[20];
    private ParticleSystem _particleSystem;
    private DiceMover _diceMover;
    private SpriteRenderer _spriteRenderer;
    private RectTransform _rectTransform;

    [SerializeField]
    private Modifier[] _modifiers = new Modifier[0];
    private int _result = 1;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _particleSystem = GetComponent<ParticleSystem>();
        _diceMover = GetComponent<DiceMover>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        _diceMover.OnMovementFinished += ShowResult;
    }
    private void ShowResult()
    {
        _result = Random.Range(1, 20);
        _spriteRenderer.sprite = _sprites[_result - 1];
        if (_modifiers.Length > 0)
            ApplyModifiers();
        _particleSystem.Play();
    }
    private void ApplyModifiers()
    {
        int modifierSum = 0;
        foreach (var modifier in _modifiers)
        {
            modifierSum += modifier.Value;
        }
        _result = Mathf.Clamp(_result += modifierSum, 1, _sprites.Length);
        PlayModifyingAnim();
    }
    private void PlayModifyingAnim()
    {
        Vector3 originalScale = _rectTransform.localScale;
        Vector3 scaleTo = originalScale * 2f;
        transform.DOScale(scaleTo, 1f).SetEase(Ease.OutSine).onComplete = () =>
        _spriteRenderer.sprite = _sprites[_result - 1];
        { transform.DOScale(originalScale, 1f).SetEase(Ease.OutBounce).SetDelay(0.75f); };
    }
    private void OnDisable()
    {
        _diceMover.OnMovementFinished -= ShowResult;
    }
    
}
