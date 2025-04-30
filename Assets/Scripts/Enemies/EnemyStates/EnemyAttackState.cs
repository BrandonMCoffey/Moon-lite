using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    public EnemyAttackState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Attacking Player!");
    }

    public override void Update()
    {
        base.Update();
        
        enemyBrain.EnemyMovement.StopMoving();

        // TODO: Attacking the Player

        var distance = Vector3.Distance(enemyBrain.transform.position, enemyBrain.PlayerDetector.PlayerTransform.position);
        if (!enemyBrain.PlayerDetector.CanDetectPlayer() || distance > enemyBrain.AttackRange)
        {
            enemyStateMachine.ChangeState(enemyBrain.ChaseState);
        }
    }
}
