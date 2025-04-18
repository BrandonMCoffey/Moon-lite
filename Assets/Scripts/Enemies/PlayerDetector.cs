using UnityEngine;
using SaiUtils.ImprovedTimers;

public class PlayerDetector : MonoBehaviour 
{
    [SerializeField] float detectionAngle = 45f; 
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float innerDetectionRadius = 5f;
    [SerializeField] float detectionCooldown = 1f;

    public Transform PlayerTransform { get; private set; }
    CountdownTimer detectionTimer;
    IDetectionStrategy detectionStrategy;

    void Start() 
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        PlayerTransform = Player.Instance.transform; 
        detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);    
    }

    public bool CanDetectPlayer() => detectionTimer.IsRunning || detectionStrategy.IsPlayerDetected(PlayerTransform, transform, detectionTimer);
    public void SetDetectionStrategy(IDetectionStrategy strategy) => detectionStrategy = strategy;

    private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);
        Gizmos.color = Color.blue;

        Vector3 right = transform.right;

        Vector3 direction = Quaternion.Euler(0, 0,  -detectionAngle / 2) * right;
        Gizmos.DrawRay(transform.position, direction * detectionRadius);

        direction = Quaternion.Euler(0, 0, detectionAngle / 2) * right;
        Gizmos.DrawRay(transform.position, direction * detectionRadius);
    }
}