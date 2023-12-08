using UnityEngine;

public class DiceAnimionController : MonoBehaviour
{
    private DiceMover _dice;
    private RectTransform _parentTransform;
    private Animator _animator;
    private int _dotParemeterHash;
    private int _wedgeParameterHash;
    private Vector2 _currentSegmentSpeedRange = Vector2.zero;
    private void Awake()
    {
        _dice = GetComponent<DiceMover>();
        _parentTransform = GetComponentInParent<RectTransform>();
        _animator = GetComponent<Animator>();
        _dotParemeterHash = Animator.StringToHash("Dot");
        _wedgeParameterHash = Animator.StringToHash("Wedge");
    }
    private void OnEnable()
    {
        _dice.OnMoveDirectionSet += SetAnimationValues;
        _dice.OnSpeedTChanged += SetAnimationSpeed;
        _dice.OnMovementFinished += TurnOff;
        _dice.OnMovementStarted += TurnOn;
    }
    private void TurnOn()
    {
        _animator.enabled = true;
    }
    private void TurnOff()
    {
        _animator.enabled = false;
    }
    private void SetAnimationSpeed(float t)
    {
        _animator.speed = Mathf.Lerp(_currentSegmentSpeedRange.x, _currentSegmentSpeedRange.y, t);
    }
    private void SetAnimationValues(Vector2 diceMoveDirection, Vector2 normalizedEventMarksInSegment)
    {
        _currentSegmentSpeedRange = normalizedEventMarksInSegment;
        float dot = Vector2.Dot(_parentTransform.up, diceMoveDirection) > 0 ? -1 : 1;
        float wedge = MathUtils.Wedge(_parentTransform.up, diceMoveDirection) > 0 ? -1 : 1;
        _animator.SetFloat(_dotParemeterHash, dot);
        _animator.SetFloat(_wedgeParameterHash, wedge);
    }
    private void OnDisable()
    {
        _dice.OnMoveDirectionSet -= SetAnimationValues;
        _dice.OnSpeedTChanged -= SetAnimationSpeed;
        _dice.OnMovementFinished -= TurnOff;
        _dice.OnMovementStarted -= TurnOn;
    }
}
