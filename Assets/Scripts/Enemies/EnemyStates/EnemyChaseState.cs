using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody) : base(enemyBrain, enemyStateMachine, enemyRigidbody) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Chase State Entered");
    }
}
