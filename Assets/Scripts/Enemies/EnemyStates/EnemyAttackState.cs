using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    PlayerMovementCustom playerMovement;
    private float lastRepositionTime;
    private float nextRepositionDelay;
    private bool isStrafing;
    private float strafeDirection = 1f;

    public EnemyAttackState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        //Debug.Log($"{Time.time}: Entering Attack State");
        if (!enemyBrain.CanAttack) enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);
        
        enemyBrain.SetCurrentStateText("AttackState");

        playerMovement = enemyBrain.PlayerDetector.PlayerTransform.GetComponent<PlayerMovementCustom>();
        if (playerMovement != null)
        {
            enemyBrain.EnemyWeaponManager.Attack(playerMovement);
        }
        else
        {
            //Debug.LogWarning("PlayerMovementCustom component not found on PlayerTransform.");
            enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);
        }

        ResetRepositionTimer();
        isStrafing = Random.value < enemyBrain.StrafeChance; 
        strafeDirection = (Random.value < 0.5f) ? 1f : -1f; 
    }

    public override void Update()
    {
       base.Update();

        if (playerMovement == null || !enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            enemyStateMachine.ChangeState(enemyBrain.SearchState);
            return;
        }

        float distanceToPlayer = enemyBrain.GetDistanceFromPlayer(); // Use helper

        if (distanceToPlayer > enemyBrain.AttackRange)
        {
            enemyStateMachine.ChangeState(enemyBrain.ChaseState);
            return;
        }
        
        enemyBrain.EnemyMovement.EnsureFacingTarget(playerMovement.transform);

        // --- Attack Logic ---
        enemyBrain.EnemyWeaponManager.Attack(playerMovement); 

        // --- Movement Logic ---
        if (Time.time > lastRepositionTime + nextRepositionDelay)
        {
            DecideNextMovementAction(distanceToPlayer);
            ResetRepositionTimer();
        }

        if (isStrafing)
        {
            enemyBrain.EnemyMovement.Strafe(strafeDirection);
            if ((strafeDirection > 0 && enemyBrain.EnemyMovement.RightWallCollided) || (strafeDirection < 0 && enemyBrain.EnemyMovement.LeftWallCollided))
            {
                strafeDirection *= -1f; 
            }
        }
        else // Not strafing, maybe maintain preferred distance
        {
            AdjustDistance(distanceToPlayer);
        }
    }

    void DecideNextMovementAction(float currentDistance)
    {
        // Simple logic: Randomly decide to strafe or reposition/adjust distance
        if (Random.value < enemyBrain.StrafeChance) 
        {
            isStrafing = true;
            // Change strafe direction sometimes for variety
            if (Random.value < 0.3f) // 30% chance to switch direction
            {
                 strafeDirection *= -1f;
            }
        }
        else
        {
            isStrafing = false;
            // Could add more complex reposition logic here, e.g., find cover (needs external system)
            // For now, just stop strafing and rely on AdjustDistance or stand still
            enemyBrain.EnemyMovement.StopMoving(); 
        }
    }

    void AdjustDistance(float currentDistance)
    {
        float diff = currentDistance - enemyBrain.PreferredEngagementRange;
        float tolerance = 1.0f; // How close to preferred range is acceptable

        if (diff > tolerance) // Too far, move closer
        {
            var directionToPlayer = Mathf.Sign(playerMovement.transform.position.x - enemyBrain.transform.position.x);
            enemyBrain.EnemyMovement.SetHorizontalVelocity(enemyBrain.EnemyMovement.Velocity.x * directionToPlayer * 0.5f); 
        }
        else if (diff < -tolerance) // Too close, move away
        {
            var directionToPlayer = Mathf.Sign(playerMovement.transform.position.x - enemyBrain.transform.position.x);
            enemyBrain.EnemyMovement.SetHorizontalVelocity(-enemyBrain.EnemyMovement.Velocity.y * directionToPlayer * 0.5f); 
        }
        else // Within preferred range
        {
            enemyBrain.EnemyMovement.StopMoving(); // Stop horizontal movement
        }
    }

    void ResetRepositionTimer()
    {
        lastRepositionTime = Time.time;
        nextRepositionDelay = Random.Range(enemyBrain.RepositionTimeMin, enemyBrain.RepositionTimeMax);
    }
}
