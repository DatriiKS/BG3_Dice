using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DiceThrower : MonoBehaviour
{
    public event Action<List<Vector2>> OnWaypointsCalculeted;

    [SerializeField]
    private float _angleDeg;
    [SerializeField]
    private RectTransform _startingPoint;
    [SerializeField]
    private List<Vector2> _wayPoints = new List<Vector2>();
    [SerializeField]
    private float _moveAmount = 1000f;

    private Rect _boardRect;
    private RectTransform _boardTransform;
    private float _centerToTopDist;
    private float _centerToRightDist;
    private Vector2 _fromPointDirection;
    private Vector2 _currentPoint;
    private DiceMover _diceMover;
    //С #If - #EndIf очень неинтуитивно, т.к. надо постоянно включать\выключать гизмо и весь смысл этой системы теряется. Так что так.
    private bool _isInPlayMode = false;
    private bool _validThrow = true;
    private void Awake()
    {
        _diceMover = GetComponentInChildren<DiceMover>();
    }
    private void OnEnable()
    {
        _diceMover.OnMovementStarted += Toggle;
        _diceMover.OnMovementFinished += Toggle;
    }
    private void OnDisable()
    {
        _diceMover.OnMovementStarted -= Toggle;
        _diceMover.OnMovementFinished -= Toggle;
    }
    private void Toggle() => 
        _validThrow = !_validThrow;
    public void ThrowDice()
    {
        if (!_validThrow)
            return;
        _isInPlayMode=true;
        InitializeValues();
        _wayPoints.Clear();
        _wayPoints.Add(_currentPoint);
        CalculatePath();
        OnWaypointsCalculeted?.Invoke(_wayPoints);
    }
    private void OnDrawGizmos()
    {
        if (!_isInPlayMode)
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            InitializeValues();
            _wayPoints.Clear();
            _wayPoints.Add(_currentPoint);
            CalculatePath();
            for (int i = 1; i < _wayPoints.Count; i++)
            {
                Gizmos.DrawLine(_wayPoints[i - 1], _wayPoints[i]);
            }
        }
    }
    
    private void InitializeValues()
    {
        _boardRect = GetComponent<RectTransform>().rect;
        _boardTransform = GetComponent<RectTransform>();
        _centerToTopDist = _boardRect.height * 0.5f;
        _centerToRightDist = _boardRect.width * 0.5f;
        _fromPointDirection = new Vector2(Mathf.Cos(_angleDeg * Mathf.Deg2Rad), Mathf.Sin(_angleDeg * Mathf.Deg2Rad));
        _currentPoint = _startingPoint.localPosition;
    }
    private void CalculatePath()
    {
        float localMoveAmount = _moveAmount;
        if (!_boardRect.Contains(_currentPoint))
            return;
        while (localMoveAmount > 0)
        {
            Vector2 uncutPoint = _currentPoint + (_fromPointDirection * localMoveAmount);
            if (_boardRect.Contains(uncutPoint))
            {
                localMoveAmount = 0;
                _fromPointDirection = Vector2.zero;
                if (_wayPoints.Exists(a => a == uncutPoint))
                    return;
                _wayPoints.Add(uncutPoint);
            }
            else
            {
                (Vector2, bool) newPosTuple = CalculateNewPos(_boardRect, _currentPoint, uncutPoint);
                Vector2 newPosition = newPosTuple.Item1;
                bool isNewPosAZeroAngle = newPosTuple.Item2;
                Vector2 newDirection = CalculateReflectedDirection(isNewPosAZeroAngle, newPosition, _currentPoint, _centerToRightDist).normalized;
                localMoveAmount -= (newPosition - _currentPoint).magnitude;
                _wayPoints.Add(newPosition);
                _currentPoint = newPosition;
                _fromPointDirection = newDirection;
            }
        }
    }
    private (Vector2, bool) CalculateNewPos(Rect boardRect, Vector2 currentPoint, Vector2 uncutPoint)
    {
        Vector2 newPos;
        if (TryGetTwoClosestToLineVertecies(boardRect, currentPoint, uncutPoint, out Vector2 closestPositiveAngleVertexPos, out Vector2 closestNegativeVertexPos, out Vector2 zeroAnglePos))
        {
            newPos = GetIntersectionPoint(currentPoint, uncutPoint, closestPositiveAngleVertexPos, closestNegativeVertexPos);
            return (newPos, false);
        }
        else
        {
            newPos = zeroAnglePos;
            return (newPos, true);
        }
    }
    private Vector2 CalculateReflectedDirection(bool isZeroAnglePos, Vector2 newPosition, Vector2 currentPoint,float centerToRightDist)
    {
        Vector2 directionFromCurrentTonNew = (newPosition - currentPoint).normalized;
        double absluteX = Math.Abs(newPosition.x);
        double roundedX = Math.Round(absluteX, 1);
        double roundedHalfWidth = Math.Round(centerToRightDist, 1);
        if (!isZeroAnglePos)
        {
            if (roundedX == roundedHalfWidth)
            {
                return new Vector2(directionFromCurrentTonNew.x * -1, directionFromCurrentTonNew.y);
            }
            else
            {
                return new Vector2(directionFromCurrentTonNew.x, directionFromCurrentTonNew.y * -1);
            }
        }
        else
        {
            return directionFromCurrentTonNew * -1;
        }
    }

    private Vector2 GetIntersectionPoint(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
    {
        float numerator = (D.x - C.x) * (C.y - A.y) - (D.y - C.y) * (C.x - A.x);
        float denominator = (D.x - C.x) * (B.y - A.y) - (D.y - C.y) * (B.x - A.x);
        float alpha = numerator / denominator;

        Vector2 result = new Vector2();
        result.x = A.x + alpha * (B.x - A.x);
        result.y = A.y + alpha * (B.y - A.y);
        return result;
    }

    private bool TryGetTwoClosestToLineVertecies(
        Rect boardRect, Vector2 currentPoint, Vector2 uncutPoint, out Vector2 closestPositive, out Vector2 closestNegative, out Vector2 zeroAnglePos
        )
    {
        List<Vector2> allRectVerticiesPos = new List<Vector2>()
        {
            new Vector2(boardRect.xMin, boardRect.yMin),
            new Vector2(boardRect.xMax, boardRect.yMin),
            new Vector2(boardRect.xMin, boardRect.yMax),
            new Vector2(boardRect.xMax, boardRect.yMax)
        };
        GetValidRectVerticiesPos(allRectVerticiesPos, currentPoint);
        List<float> currentToVertAngles = new List<float>();
        Dictionary<float, Vector3> anglePosVerticies = new Dictionary<float, Vector3>();
        Vector2 currentToEndDirection = (uncutPoint - currentPoint);
        foreach (var vertex in allRectVerticiesPos)
        {
            Vector2 currentToVertexDirection = (vertex - currentPoint);
            float pointToVertexAngle = Vector2.SignedAngle(currentToEndDirection, currentToVertexDirection);
            currentToVertAngles.Add(pointToVertexAngle);
            anglePosVerticies.Add(pointToVertexAngle, vertex);
        }
        float closestToZeroNegativeAngle = currentToVertAngles.Where(a => a < 0).Max();
        float closestToZeroPositiveAngle = currentToVertAngles.Where(a => a > 0).Min();
        closestNegative = anglePosVerticies[closestToZeroNegativeAngle];
        closestPositive = anglePosVerticies[closestToZeroPositiveAngle];

        bool noZeroPoints = !currentToVertAngles.Exists(a => a == 0);
        if (noZeroPoints)
        {
            zeroAnglePos = Vector2.zero;
        }
        else 
        {
            zeroAnglePos = anglePosVerticies[currentToVertAngles.Find(a => a == 0)];
        }

        return noZeroPoints ? true : false;
    }
    private void GetValidRectVerticiesPos(List<Vector2> allRectVerticiesPos, Vector2 currentPoint) 
    {
        Vector2? currentPointAtVertexPos = allRectVerticiesPos.SingleOrDefault(a => a == currentPoint);
        if (currentPointAtVertexPos != null) 
        {
            allRectVerticiesPos.Remove(currentPoint);
        }
    }
}
