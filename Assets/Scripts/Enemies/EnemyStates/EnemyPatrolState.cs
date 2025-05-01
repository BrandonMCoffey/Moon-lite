using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyPatrolState : EnemyBaseState
{
    public EnemyPatrolState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void Update()
    {
        base.Update();
        enemyBrain.EnemyMovement.Patrol();

        if (enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            enemyStateMachine.ChangeState(enemyBrain.ChaseState);
        }
    }
}
