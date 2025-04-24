using UnityEngine;
using SaiUtils.ImprovedTimers;

public class PlayerDetector : MonoBehaviour 
{
    [SerializeField] private float detectionAngle = 45f; 
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float innerDetectionRadius = 5f;
    [SerializeField] private float detectionCooldown = 1f;
    [SerializeField] private LayerMask playerLayerMask;
    public LayerMask PlayerLayer => playerLayerMask;
    [SerializeField] private LayerMask obstacleLayerMask;
    public LayerMask ObstacleLayer => obstacleLayerMask;

    public Transform PlayerTransform { get; private set; }
    private CountdownTimer detectionTimer;
    private IDetectionStrategy detectionStrategy;

    private void Start() 
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        PlayerTransform = Player.Instance.transform; 
        detectionStrategy = new ConeDetectionStrategy(playerLayerMask, obstacleLayerMask, detectionAngle, detectionRadius, innerDetectionRadius);    
    }

    public bool CanDetectPlayer() => detectionTimer.IsRunning || detectionStrategy.IsPlayerDetected(PlayerTransform, transform, detectionTimer);
    public void SetDetectionStrategy(IDetectionStrategy strategy) => detectionStrategy = strategy;

    private void OnDrawGizmosSelected() 
    {
        if (detectionStrategy != null) 
        {
            detectionStrategy.DrawGizmos(transform);
        }
    }
}