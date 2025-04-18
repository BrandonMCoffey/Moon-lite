using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyWanderState : EnemyBaseState
{
    public EnemyWanderState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody) : base(enemyBrain, enemyStateMachine, enemyRigidbody) { }
}
