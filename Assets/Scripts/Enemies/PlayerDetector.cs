using UnityEngine;
using SaiUtils.ImprovedTimers;

public class PlayerDetector : MonoBehaviour 
{
    [SerializeField] float detectionAngle = 45f; 
    [SerializeField] float detectionRadius = 10f;
    [SerializeField] float innerDetectionRadius = 5f;
    [SerializeField] float detectionCooldown = 1f;

    public Transform Player { get; private set; }
    CountdownTimer detectionTimer;
    IDetectionStrategy detectionStrategy;

    void Start() 
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        Player = GameObject.FindGameObjectWithTag("Player").transform;
        detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);    
    }

    public bool CanDetectPlayer() => detectionTimer.IsRunning || detectionStrategy.IsPlayerDetected(Player, transform, detectionTimer);
    public void SetDetectionStrategy(IDetectionStrategy strategy) => detectionStrategy = strategy;
}