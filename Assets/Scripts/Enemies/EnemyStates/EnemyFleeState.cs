using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyFleeState : EnemyBaseState
{
    public EnemyFleeState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody) : base(enemyBrain, enemyStateMachine, enemyRigidbody) { }
}
