using UnityEngine;
using SaiUtils.ImprovedTimers;

public interface IDetectionStrategy
{
    bool IsPlayerDetected(Transform player, Transform detector, CountdownTimer timer);
    void DrawGizmos(Transform detector);
}