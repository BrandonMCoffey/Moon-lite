using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyBlastToMoveState : EnemyBaseState
{
    public EnemyBlastToMoveState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        if (!enemyBrain.CanBlastTo) enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);

        enemyBrain.SetCurrentStateText("BlastToMoveState");
        enemyBrain.SetIgnoreCallForReinforcement(true);
    }

    public override void Update()
    {
        var canMove = enemyBrain.EnemyMovement.MoveTowards(enemyBrain.BlastToPosition);
        if (!canMove)
        {
            enemyStateMachine.ChangeState(enemyBrain.BlastToState);
        }

        var distanceToBlastToPosition = Vector3.Distance(enemyBrain.transform.position, enemyBrain.BlastToPosition);
        if (Mathf.Abs(distanceToBlastToPosition) < 0.2f)
        {
            enemyStateMachine.ChangeState(enemyBrain.PatrolState);
        }
    }

}
