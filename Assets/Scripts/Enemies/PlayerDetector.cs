using UnityEngine;
using SaiUtils.ImprovedTimers;
using Sirenix.OdinInspector;

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
    [SerializeField, ReadOnly] private bool _canSeePlayer = false;

    public Transform PlayerTransform { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; } = Vector3.zero;
    private CountdownTimer detectionTimer;
    private IDetectionStrategy detectionStrategy;

    bool _doOnce = false;

    private void Start() 
    {
        detectionTimer = new CountdownTimer(detectionCooldown);
        PlayerTransform = Player.Instance.transform; 
        detectionStrategy = new ConeDetectionStrategy(playerLayerMask, obstacleLayerMask, detectionAngle, detectionRadius, innerDetectionRadius);    
    }

    public bool CanDetectPlayer() 
    {
        if (detectionTimer.IsRunning)
        {
            return _canSeePlayer;
        }
        _canSeePlayer = detectionStrategy.IsPlayerDetected(PlayerTransform, transform, detectionTimer);
        detectionTimer.Start();
        return _canSeePlayer;
    }
    public void SetDetectionStrategy(IDetectionStrategy strategy) => detectionStrategy = strategy;

    private void OnDrawGizmosSelected() 
    {
        if (detectionStrategy != null) 
        {
            detectionStrategy.DrawGizmos(transform);
        }
    }
    
    void Update()
    {
        if (!CanDetectPlayer())
        {
            if (!_doOnce) 
            {
                LastKnownPlayerPosition = PlayerTransform.position;
                _doOnce = true;
            }
        }
        else
        {
            _doOnce = false;
        }
    }
}