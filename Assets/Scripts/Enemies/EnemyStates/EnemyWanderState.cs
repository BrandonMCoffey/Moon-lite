using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.ImprovedTimers;

public class EnemyWanderState : EnemyBaseState
{
    CountdownTimer wanderTimer;
    CountdownTimer reactionCooldownTimer;
    float facingDirection;
    float pitCheckDistance = 5f; 
    LayerMask groundLayerMask; 

    Vector3 wanderPosition;

    public EnemyWanderState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody, float facingDirection, LayerMask groundLayer) : base(enemyBrain, enemyStateMachine, enemyRigidbody) 
    {
        this.facingDirection = facingDirection;
        this.enemyRigidbody = enemyRigidbody;
        this.groundLayerMask = groundLayer;
    }

    public override void OnEnter()
    {
        enemyRigidbody.velocity = Vector3.zero; 

        wanderTimer = new CountdownTimer(Random.Range(3, 5));
        wanderTimer.OnTimerEnd += () =>
        {
            PickNewWanderPosition();
            ResetWanderTimer();
        };
        ResetWanderTimer();

        reactionCooldownTimer = new CountdownTimer(0.5f); // cooldown before reacting to another obstacle
    }

    public override void OnExit()
    {
        wanderTimer.Stop();
    }

    public override void Update()
    {
        // Update facing direction based on the wander direction
        if ((wanderPosition - enemyRigidbody.position).x > 0)
        {
            facingDirection = 1f; 
            enemyBrain.transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
        else if ((wanderPosition - enemyRigidbody.position).x < 0)
        {
            facingDirection = -1f; 
            enemyBrain.transform.rotation = Quaternion.Euler(0, 180, 0); 
        }
    }

    public override void FixedUpdate()
    {
        // Raycast side to side to check for walls
        Vector3 wallDirection = Vector3.right * facingDirection;
        var foundWall = Physics.Raycast(enemyRigidbody.position, wallDirection, out RaycastHit hit, 1.05f);

        // Raycast in a diagonal direction near the ground to check for pits
        var forwardOffset = wallDirection * pitCheckDistance;
        var pitCheckOrigin = enemyRigidbody.position + forwardOffset;
        var foundPit = !Physics.Raycast(pitCheckOrigin, Vector3.down, out RaycastHit pitHit, 1.05f, groundLayerMask); 

        if ((foundWall || foundPit) && !reactionCooldownTimer.IsRunning)
        {
            enemyRigidbody.velocity = Vector3.zero;

            // 50-50 chance to jump over the wall if one is detected
            if (foundWall && Random.value > 0f)
            {
                enemyRigidbody.AddForce(new Vector3(facingDirection * 5f, 7f, 0f), ForceMode.Impulse);
            }
            else
            {
                Vector3 newDirection = -wallDirection * 5f;
                PickNewWanderPosition(newDirection);
                ResetWanderTimer();
            }

            reactionCooldownTimer.Reset(3f);
            reactionCooldownTimer.Start();
        }
        else if (wanderTimer.IsRunning)
        {
            // Move towards the wander position
            Vector3 direction = (wanderPosition - enemyRigidbody.position).normalized;
            enemyRigidbody.MovePosition(enemyRigidbody.position + direction * Time.fixedDeltaTime * 2f); // Adjust speed as needed
        }

        if (Vector3.Distance(enemyRigidbody.position, wanderPosition) < 0.2f)
        {
            PickNewWanderPosition();
            ResetWanderTimer();
        }
    }

    private void PickNewWanderPosition(Vector3 originForRandomPosition = default)
    {
        originForRandomPosition = originForRandomPosition == default ? enemyRigidbody.position : originForRandomPosition;

        // Generate a random position within a certain range, restricted to horizontal movement
        Vector3 randomOffset = new Vector3(Random.Range(-5f, 5f), 0f, 0f);
        Vector3 randomPosition = originForRandomPosition + randomOffset;

        wanderPosition = randomPosition;
    }

    private void ResetWanderTimer()
    {
        wanderTimer.Reset(Random.Range(3, 5));
        wanderTimer.Start();
    }
}
