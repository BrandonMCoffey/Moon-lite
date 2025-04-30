using UnityEngine;
using SaiUtils.ImprovedTimers;

public class ConeDetectionStrategy : IDetectionStrategy
{
    private readonly float detectionAngle;
    private readonly float detectionRadius;
    private readonly float innerDetectionRadius;
    private readonly int numOfRays;
    private readonly LayerMask playerLayer;
    private readonly LayerMask groundLayer;

    public ConeDetectionStrategy(LayerMask playerLayer, LayerMask groundLayer, float detectionAngle, float detectionRadius, float innerDetectionRadius, int numOfRays = 10)
    {
        this.detectionAngle = detectionAngle;
        this.detectionRadius = detectionRadius;
        this.innerDetectionRadius = innerDetectionRadius;
        this.numOfRays = numOfRays;
        this.playerLayer = playerLayer;
        this.groundLayer = groundLayer;
    }

    public bool IsPlayerDetected(Transform player, Transform detector, CountdownTimer timer)
    {
        if (timer.IsRunning) return false;

        Vector3 toPlayer = player.position - detector.position;
        float distance = toPlayer.magnitude;

        bool withinCone = Vector3.Angle(detector.right, toPlayer.normalized) < detectionAngle / 2 && distance < detectionRadius;
        bool withinInnerRadius = distance < innerDetectionRadius;

        if (withinCone || withinInnerRadius)
        {
            Vector3 baseDirection = detector.right;
            float halfAngle = detectionAngle / 2f;
            float angleStep = detectionAngle / numOfRays;

            for (int i = 0; i < numOfRays; i++)
            {
                float angleOffset = -halfAngle + (angleStep * i);
                Vector3 rayDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * baseDirection;

                if (Physics.Raycast(detector.position, rayDirection, out RaycastHit hit, detectionRadius, playerLayer))
                {
                    if (Physics.Raycast(detector.position, rayDirection, out _, detectionRadius, groundLayer))
                    {
                        // there is a ground object in the way, ignore this ray
                        Debug.DrawRay(detector.position, rayDirection * detectionRadius, Color.yellow);
                    }
                    else 
                    {
                        Debug.DrawRay(detector.position, rayDirection * detectionRadius, Color.red);
                        var playerRef = hit.collider.GetComponent<Player>();
                        if (playerRef != null)
                        {
                            Debug.Log($"Player detected by raycast: {hit.collider.name}");
                            timer.Start();
                            return true;
                        }
                    }  
                }
            }
        }

        return false;   
    }

    public void DrawGizmos(Transform detector)
    {

        // Gizmos.color = Color.yellow;

        // Vector3 baseDirection = detector.right; // treat right as forward
        // float halfAngle = detectionAngle / 2f;
        // float angleStep = detectionAngle / numOfRays;

        // for (int i = 0; i <= numOfRays; i++) // include the final ray
        // {
        //     float angleOffset = -halfAngle + (angleStep * i);
        //     Vector3 rayDirection = Quaternion.AngleAxis(angleOffset, Vector3.forward) * baseDirection;
        //     Gizmos.DrawRay(detector.position, rayDirection.normalized * detectionRadius);
        // }

        // // Draw inner detection radius as a wire circle
        // Gizmos.color = Color.cyan;
        // Gizmos.DrawWireSphere(detector.position, innerDetectionRadius);
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(detector.position, detectionRadius);

    }

}
