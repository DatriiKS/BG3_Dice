using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DiceMover : MonoBehaviour
{ 
    public event Action<Vector2,Vector2> OnMoveDirectionSet;
    public event Action<float> OnSpeedTChanged;
    public event Action OnMovementFinished;
    public event Action OnMovementStarted;

    [SerializeField]
    private float _travelTime = 3f;
    private DiceThrower _thrower;
    private Vector2 _zeroToOneRange = new Vector2(0f, 1f);
    private void Awake()
    {
        _thrower = GetComponentInParent<DiceThrower>();
    }
    private void OnEnable()
    {
        _thrower.OnWaypointsCalculeted += CalculateEventMarks;
    }
    private IEnumerator MoveRoutine(List<float> normalizedEventMarks,List<Vector2> waypoints)
    {
        OnMovementStarted?.Invoke();
        for (int i = 1; i < waypoints.Count; i++)
        {
            Vector2 currentPoint = waypoints[i -1];
            Vector2 nextPoint = waypoints[i];
            float elapsedTime = 0;
            float timeForCurrentSegment = MathUtils.Remap(normalizedEventMarks[i] - normalizedEventMarks[i-1], _zeroToOneRange, new Vector2(0, _travelTime));
            Vector2 normalizedSpeedRangeForSegment = new Vector2(1 - normalizedEventMarks[i-1], 1 - normalizedEventMarks[i]);
            OnMoveDirectionSet?.Invoke((currentPoint - nextPoint).normalized, normalizedSpeedRangeForSegment);
            while (elapsedTime < timeForCurrentSegment)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / timeForCurrentSegment;
                transform.localPosition = Vector2.Lerp(currentPoint, nextPoint, t);
                OnSpeedTChanged?.Invoke(t);
                yield return null;
            }
            transform.localPosition = waypoints[i];
        }
        OnMovementFinished?.Invoke();
    }
    private void CalculateEventMarks(List<Vector2> waypoints)
    {
        (float, List<float>) distanceInfoTuple = GetDistanceInfo(waypoints);
        float sumDistance = distanceInfoTuple.Item1;

        List<float> distanceEventMarks = distanceInfoTuple.Item2;
        List<float> normalizedEventMarks = new List<float>();
        for (int i = 0; i < distanceEventMarks.Count; i++)
        {
            float eventMarkOnZeroToOneScale = MathUtils.Remap(distanceEventMarks[i], new Vector2(0, sumDistance), _zeroToOneRange);
            normalizedEventMarks.Add(eventMarkOnZeroToOneScale);
        }
        StartCoroutine(MoveRoutine(normalizedEventMarks,waypoints));
    }

    private (float, List<float>) GetDistanceInfo(List<Vector2> wayPoints)
    {
        float sum = 0;
        List<float> distanceMarks = new List<float> { 0 };
        for (int i = 1; i < wayPoints.Count; i++)
        {
            float distance = (Vector2.Distance(wayPoints[i - 1], wayPoints[i]));
            sum += distance;
            distanceMarks.Add(sum);
        }
        return (sum, distanceMarks);
    }
    private void OnDisable()
    {
        _thrower.OnWaypointsCalculeted -= CalculateEventMarks;
    }
}
