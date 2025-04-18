using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyBaseState : IState
{
    protected EnemyBrain enemyBrain;
    protected StateMachine enemyStateMachine;
    protected Rigidbody enemyRigidbody;

    public EnemyBaseState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, Rigidbody enemyRigidbody)
    {
        this.enemyBrain = enemyBrain;
        this.enemyStateMachine = enemyStateMachine;
        this.enemyRigidbody = enemyRigidbody;
    }

    public void FixedUpdate() { }

    public void OnEnter() { }

    public void OnExit() { }

    public void Update() { }
}