using UnityEngine;
using SaiUtils.ImprovedTimers;

public class PlayerDetector : MonoBehaviour 
{
    [SerializeField] float detectionAngle = 45f; 
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float innerDetectionRadius = 5f;
    [SerializeField] float detectionCooldown = 1f;
    [SerializeField] LayerMask playerLayerMask;
    public LayerMask PlayerLayer => playerLayerMask;
    [SerializeField] LayerMask obstacleLayerMask;
    public LayerMask ObstacleLayer => obstacleLayerMask;

    public Transform PlayerTransform { get; private set; }
    CountdownTimer detectionTimer;
    IDetectionStrategy detectionStrategy;

    void Start() 
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