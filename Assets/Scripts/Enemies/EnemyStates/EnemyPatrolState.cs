using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyPatrolState : EnemyBaseState
{
    public EnemyPatrolState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, EnemyMovement enemyMovement)
        : base(enemyBrain, enemyStateMachine, enemyMovement) { }

    public override void Update()
    {
        base.Update();
        enemyMovement.Patrol();
    }
}
