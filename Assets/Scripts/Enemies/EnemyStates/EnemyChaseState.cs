using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.ImprovedTimers;

public class EnemyChaseState : EnemyBaseState
{
    private float playerRememberTime;
    private float chaseSpeed;
    private Transform playerTransform;
    private LayerMask groundLayerMask;
    private float pitCheckDistance;

    private CountdownTimer playerRememberTimer;
    private CountdownTimer checkForObstacleTimer;
    private bool canChasePlayer;

    public EnemyChaseState(
        EnemyBrain enemyBrain, 
        StateMachine enemyStateMachine, 
        Rigidbody enemyRigidbody, 
        float playerRememberTime, 
        float chaseSpeed, 
        LayerMask groundLayerMask,
        float pitCheckDistance) 
    : base(enemyBrain, enemyStateMachine, enemyRigidbody) 
    {
        this.playerRememberTime = playerRememberTime;
        this.chaseSpeed = chaseSpeed;
        this.groundLayerMask = groundLayerMask;
        this.pitCheckDistance = pitCheckDistance;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Chase State Entered");
        var detector = enemyBrain.PlayerDetector;
        detector.SetDetectionStrategy(new ConeDetectionStrategy(detector.PlayerLayer, detector.ObstacleLayer, 60f, 15f, 3f, 10));
        playerTransform = enemyBrain.PlayerDetector.PlayerTransform;

        playerRememberTimer = new CountdownTimer(playerRememberTime);
        
        playerRememberTimer.OnTimerEnd += () => {
            if (enemyBrain.PlayerDetector.CanDetectPlayer()) return;
            enemyBrain.StateMachine.ChangeState(enemyBrain.EnemyWanderState);
        };

        checkForObstacleTimer = new CountdownTimer(0.5f);
    }

    public override void Update()
    {
        bool detected = enemyBrain.PlayerDetector.CanDetectPlayer();
        
        if (detected) playerRememberTimer.Stop();
        else if (!playerRememberTimer.IsRunning) playerRememberTimer.Start();

        canChasePlayer = detected || playerRememberTimer.IsRunning;

    }

    public override void FixedUpdate()
    {
        if (canChasePlayer) ChasePlayer();

    }

    public override void OnExit()
    {
        base.OnExit();
        if (playerRememberTimer.IsRunning) playerRememberTimer.Stop();
        Debug.Log("Chase State Exited");
    }

    private void ChasePlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - enemyRigidbody.position).normalized;
        directionToPlayer.y = 0; // Ignore vertical movement
        enemyRigidbody.velocity = directionToPlayer * chaseSpeed;

        
        if (directionToPlayer.x > 0) enemyBrain.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (directionToPlayer.x < 0) enemyBrain.transform.rotation = Quaternion.Euler(0, 180, 0);

        // Raycast side to side to check for walls
        Vector3 wallDirection = enemyBrain.transform.right;

        var foundWall = Physics.Raycast(enemyRigidbody.position, wallDirection, out _, 1.05f, groundLayerMask); // raycast from the middle of the enemy
        Debug.DrawRay(enemyRigidbody.position, wallDirection * 1.05f, Color.red);
        if (!foundWall) 
        {
            foundWall = Physics.Raycast(enemyRigidbody.position + enemyRigidbody.transform.up * 0.3f, wallDirection, out _, 1.05f, groundLayerMask); // raycast from the top of the enemy
            Debug.DrawRay(enemyRigidbody.position + enemyRigidbody.transform.up * 0.3f, wallDirection * 1.05f, Color.red);
        }
        if (!foundWall) 
        {
            foundWall = Physics.Raycast(enemyRigidbody.position - enemyRigidbody.transform.up * 0.3f, wallDirection, out _, 1.05f, groundLayerMask); // raycast from the bottom of the enemy
            Debug.DrawRay(enemyRigidbody.position - enemyRigidbody.transform.up * 0.3f, wallDirection * 1.05f, Color.red);
        }
        
        
        // Raycast in a diagonal direction near the ground to check for pits
        var forwardOffset = wallDirection * pitCheckDistance;
        var pitCheckOrigin = enemyRigidbody.position + forwardOffset;
        var foundPit = !Physics.Raycast(pitCheckOrigin, Vector3.down, out _, 1.05f, groundLayerMask); 
        Debug.DrawRay(pitCheckOrigin, Vector3.down * 1.05f, Color.blue);

        if ((foundWall) && !checkForObstacleTimer.IsRunning)
        {
        
            Debug.Log("Obstacle detected, jumping!");
            enemyRigidbody.velocity = Vector3.zero;
            // jump upwards (up) and towards the player (right)
            //Vector3 jumpDirection = new Vector3(directionToPlayer.x, 1f, 0).normalized;
            //enemyRigidbody.AddForce(jumpDirection * 15f, ForceMode.Impulse);

            enemyRigidbody.AddForce(new Vector3(directionToPlayer.x, 30f, 0f), ForceMode.Impulse);

            checkForObstacleTimer.Reset(1f);
            checkForObstacleTimer.Start();
        }
        else
        {
            // continue moving towards the player
            enemyRigidbody.velocity = directionToPlayer * chaseSpeed;
        }
    }
}
