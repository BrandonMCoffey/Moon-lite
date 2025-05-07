using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyPickRetreatPosState : EnemyBaseState
{
    
    public EnemyPickRetreatPosState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        if (!enemyBrain.CanRetreat) enemyBrain.StateMachine.ChangeState(enemyBrain.PatrolState);

        enemyBrain.SetCurrentStateText("PickRetreatPosState");
        enemyBrain.SetIgnoreCallForReinforcement(true);

        var randNumber = Random.Range(0f, 1f);
        if (randNumber < enemyBrain.CallForReinforcementsChance)
        {
            enemyBrain.StateMachine.ChangeState(enemyBrain.CallForReinforcementState);
        }
        else
        {
            var directionToPlayer = (enemyBrain.PlayerDetector.PlayerTransform.position - enemyBrain.transform.position).normalized;
            var retreatDirection = -directionToPlayer; 

            // pick a random position in the retreat direction
            var retreatPosition = enemyBrain.transform.position + retreatDirection * enemyBrain.RetreatDistance;
            
            // set the retreatPosition and change the state to urgently move towards it
            enemyBrain.SetBlastToPosition(retreatPosition);
            enemyStateMachine.ChangeState(enemyBrain.BlastToMoveState);
        }
    }

    public override void Update()
    {
        base.Update();
    }
}
