using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyRetreatState : EnemyBaseState
{
    public EnemyRetreatState(EnemyBrain enemyBrain, StateMachine enemyStateMachine)
    : base(enemyBrain, enemyStateMachine) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log($"{Time.time}: Entering Retreat State");
    }

    public override void Update()
    {
        base.Update();
    }
}
