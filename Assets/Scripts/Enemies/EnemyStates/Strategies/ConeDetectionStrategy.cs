using UnityEngine;
using SaiUtils.ImprovedTimers;

public class ConeDetectionStrategy : IDetectionStrategy
{
    readonly float detectionAngle;
    readonly float detectionRadius;
    readonly float innerDetectionRadius;

    public ConeDetectionStrategy(float detectionAngle, float detectionRadius, float innerDetectionRadius)
    {
        this.detectionAngle = detectionAngle;
        this.detectionRadius = detectionRadius;
        this.innerDetectionRadius = innerDetectionRadius;
    }

    public bool IsPlayerDetected(Transform player, Transform detector, CountdownTimer timer)
    {
        if (timer.IsRunning) return false; // Timer is running, no detection allowed

        var directionToPlayer = (player.position - detector.position).normalized;
        var angleToPlayer = Vector3.Angle(directionToPlayer, detector.right);

        // if the player is not within the detection angle + outer radius (aka the cone)
        // or is within the inner radius, return false
        if ((!(angleToPlayer < detectionAngle / 2) || !(directionToPlayer.magnitude < detectionRadius)) 
            && !(directionToPlayer.magnitude < innerDetectionRadius))
            return false; 

        timer.Start();
        return true;
    }
}