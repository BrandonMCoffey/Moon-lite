using SaiUtils.StateMachine;

public class EnemyPatrolState : EnemyBaseState
{
    public EnemyPatrolState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        //Debug.Log($"{Time.time}: Entering Patrol State");
        enemyBrain.SetCurrentStateText("PatrolState");
        enemyBrain.SetIgnoreCallForReinforcement(false);
    }

    public override void Update()
    {
        base.Update();
        enemyBrain.EnemyMovement.Patrol();

        if (enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            enemyStateMachine.ChangeState(enemyBrain.ChaseState);
        }
    }
}
