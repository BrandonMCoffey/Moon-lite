using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyRetreatState : EnemyBaseState
{
    PlayerDetector _playerDetector;
    public EnemyRetreatState(EnemyBrain enemyBrain, PlayerDetector playerDetector, StateMachine enemyStateMachine, EnemyMovement enemyMovement)
        : base(enemyBrain, enemyStateMachine, enemyMovement) { 
        _playerDetector = playerDetector;
    }

    public override void Update()
    {
        base.Update();
        enemyMovement.Retreat(_playerDetector.LastKnownPlayerPosition); //pass in the last known position of the player from PlayerDetector
    }
}
