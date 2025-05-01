using SaiUtils.StateMachine;
using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    PlayerMovementCustom playerMovement;

    public EnemyAttackState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
        : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log("Attacking Player!");

        playerMovement = enemyBrain.PlayerDetector.PlayerTransform.GetComponent<PlayerMovementCustom>();
        if (playerMovement != null)
        {
            enemyBrain.EnemyWeaponManager.Attack(playerMovement);
        }
        else
        {
            Debug.LogWarning("PlayerMovementCustom component not found on PlayerTransform.");
        }
    }

    public override void Update()
    {
        base.Update();
        
        enemyBrain.EnemyMovement.StopMoving();

        // TODO: Attacking the Player
        enemyBrain.EnemyWeaponManager.Attack(playerMovement);

        var distance = Vector3.Distance(enemyBrain.transform.position, enemyBrain.PlayerDetector.PlayerTransform.position);
        if (distance > enemyBrain.AttackRange)
        {
            enemyStateMachine.ChangeState(enemyBrain.ChaseState);
        }
        else if (!enemyBrain.PlayerDetector.CanDetectPlayer())
        {
            enemyStateMachine.ChangeState(enemyBrain.PatrolState);
        }
    }
}
