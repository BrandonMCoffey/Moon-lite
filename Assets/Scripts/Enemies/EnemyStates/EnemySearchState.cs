using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.ImprovedTimers; 

public class EnemySearchState : EnemyBaseState
{
    private CountdownTimer searchTimer;
    private Vector3 lastKnownPosition;

    public EnemySearchState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) 
    {
        searchTimer = new CountdownTimer(enemyBrain.SearchTime); 
    }

    public override void OnEnter()
    {
        base.OnEnter();
        lastKnownPosition = enemyBrain.PlayerDetector.LastKnownPlayerPosition; 
        searchTimer.Start();
        //Debug.Log($"{Time.time}: Entering Search State. Last known position: {lastKnownPosition}");

        // Optional: Play a "searching" animation or sound

        searchTimer.OnTimerEnd += SetPatrol;
    }

    private void SetPatrol()
    {
        //Debug.Log("Search timer ended, returning to Patrol.");
        enemyStateMachine.ChangeState(enemyBrain.PatrolState);
        return;
    }

    public override void Update()
    {
        base.Update();

        if (enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            // Found player again! Decide whether to Chase or Attack based on range
            float distance = enemyBrain.GetDistanceFromPlayer();
            if (distance <= enemyBrain.AttackRange)
            {
                enemyStateMachine.ChangeState(enemyBrain.AttackState);
            }
            else
            {
                enemyStateMachine.ChangeState(enemyBrain.ChaseState);
            }
            return;
        }

        // Move towards the last known position
        float distanceToTarget = Vector3.Distance(enemyBrain.transform.position, lastKnownPosition);
        if (distanceToTarget > 1.0f) 
        {
            // Use the simplified MoveTowards function
            enemyBrain.EnemyMovement.MoveTowards(lastKnownPosition);
        }
        else
        {
            enemyBrain.EnemyMovement.StopMoving();
            // Optional: Add a small "look around" behavior here 
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        searchTimer.Stop(); 
        enemyBrain.EnemyMovement.StopMoving();
        //Debug.Log("Exiting Search State");

        searchTimer.OnTimerEnd -= SetPatrol;
    }
}