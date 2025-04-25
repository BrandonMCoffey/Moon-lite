using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyAttackState : EnemyBaseState
{
    public EnemyAttackState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody) : base(enemyBrain, enemyStateMachine, enemyRigidbody) { }
}
