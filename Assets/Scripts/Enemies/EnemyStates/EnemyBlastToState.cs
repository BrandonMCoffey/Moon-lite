using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.ImprovedTimers;

public class EnemyBlastToState : EnemyBaseState
{
    CountdownTimer blastTimer = new CountdownTimer(1.3f);
    public EnemyBlastToState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        if (!enemyBrain.CanBlastTo) enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);

        enemyBrain.SetCurrentStateText("BlastToState");
        enemyBrain.SetIgnoreCallForReinforcement(true);
        blastTimer.Reset(enemyBrain.TimeBlasting);
        blastTimer.Start();
    }

    public override void Update()
    {
        if (blastTimer.IsRunning)
        {
            var distanceToBlastToPosition = Vector3.Distance(enemyBrain.transform.position, enemyBrain.BlastToPosition);
            if (Mathf.Abs(distanceToBlastToPosition) < 0.2f)
            {
                enemyStateMachine.ChangeState(enemyBrain.PatrolState);
            }
            else 
            {
                enemyBrain.EnemyMovement.StopMoving();
                enemyBrain.EnemyWeaponManager.ShootTowards(enemyBrain.BlastToPosition);
            }
        }
        else 
        {
            enemyStateMachine.ChangeState(enemyBrain.BlastToMoveState);
        }
    }
}
