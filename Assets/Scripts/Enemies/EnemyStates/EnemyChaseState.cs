using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyChaseState : EnemyBaseState
{
    public EnemyChaseState(EnemyBrain enemyBrain, StateMachine enemyStateMachine) 
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        

        base.OnEnter();
        //Debug.Log($"{Time.time}: Entering Chase State");
        enemyBrain.SetCurrentStateText("ChaseState");
        enemyBrain.SetIgnoreCallForReinforcement(false);
    }

    public override void Update()
    {
        base.Update();
        if (enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            if (Vector3.Distance(enemyBrain.transform.position, enemyBrain.PlayerDetector.PlayerTransform.position) < enemyBrain.AttackRange)
            {
                enemyStateMachine.ChangeState(enemyBrain.AttackState);
            }
            else
            {
                enemyBrain.EnemyMovement.ChaseTarget(enemyBrain.PlayerDetector.PlayerTransform);
            }
        }
        else 
        {
            enemyStateMachine.ChangeState(enemyBrain.PatrolState);
        }
    }
}
