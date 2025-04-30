using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyBaseState : IState
{
    protected EnemyBrain enemyBrain;
    protected StateMachine enemyStateMachine;
    protected EnemyMovement enemyMovement;

    public EnemyBaseState(EnemyBrain enemyBrain, StateMachine enemyStateMachine, EnemyMovement enemyMovement)
    {
        this.enemyBrain = enemyBrain;
        this.enemyStateMachine = enemyStateMachine;
        this.enemyMovement = enemyMovement;
    }

    public virtual void FixedUpdate() { }

    public virtual void OnEnter() { }

    public virtual void OnExit() { }

    public virtual void Update() { }
}