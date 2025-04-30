using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyChaseState : EnemyBaseState
{
    PlayerDetector _playerDetector;
    public EnemyChaseState(EnemyBrain enemyBrain, PlayerDetector playerDetector, StateMachine enemyStateMachine, EnemyMovement enemyMovement) 
    : base(enemyBrain, enemyStateMachine, enemyMovement) 
    {
        _playerDetector = playerDetector;
    }

    public override void Update()
    {
        base.Update();
        enemyMovement.ChaseTarget(_playerDetector.PlayerTransform); 
    }
}
