using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.ImprovedTimers;

public class EnemyCallForReinforcementState : EnemyBaseState
{

    public EnemyCallForReinforcementState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        if (!enemyBrain.CanCallForReinforcement) enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);

        enemyBrain.EnemyMovement.StopMoving();
        enemyBrain.SetCurrentStateText("CallForReinforcementState");
        enemyBrain.CallForReinforcementEvent.Raise(enemyBrain.transform.position);
        enemyBrain.SetIgnoreCallForReinforcement(true);

        var directionToPlayer = (enemyBrain.PlayerDetector.PlayerTransform.position - enemyBrain.transform.position).normalized;
        var retreatDirection = -directionToPlayer; 

        // pick a random position in the retreat direction
        var retreatPosition = enemyBrain.transform.position + retreatDirection * enemyBrain.RetreatDistance;
        
        // set the retreatPosition and change the state to urgently move towards it
        enemyBrain.SetBlastToPosition(retreatPosition);
        enemyStateMachine.ChangeState(enemyBrain.BlastToMoveState);   
    }
}
