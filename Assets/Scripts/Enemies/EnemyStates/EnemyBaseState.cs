using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyBaseState : IState
{
    protected EnemyBrain enemyBrain;
    protected StateMachine enemyStateMachine;

    public EnemyBaseState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    {
        this.enemyBrain = enemyBrain;
        this.enemyStateMachine = enemyStateMachine;
    }

    public virtual void FixedUpdate() { }

    public virtual void OnEnter() { }

    public virtual void OnExit() { }

    public virtual void Update() { }
}