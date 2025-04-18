using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyBrain : MonoBehaviour 
{
    [Header("Enemy Components")]
    [SerializeField] Rigidbody enemyRigidbody;
    [SerializeField] PlayerDetector playerDetector;

    StateMachine stateMachine;
    public EnemyWanderState EnemyWanderState { get; private set; }
    public EnemyChaseState EnemyChaseState { get; private set; }
    public EnemyAttackState EnemyAttackState { get; private set; }
    public EnemyFleeState EnemyFleeState { get; private set; }

    private void Awake() {
        stateMachine = new StateMachine();
        EnemyWanderState = new EnemyWanderState(this, stateMachine, enemyRigidbody);
        EnemyChaseState = new EnemyChaseState(this, stateMachine, enemyRigidbody);
        EnemyAttackState = new EnemyAttackState(this, stateMachine, enemyRigidbody);
        EnemyFleeState = new EnemyFleeState(this, stateMachine, enemyRigidbody);

        // Wander State to Chase State and vice versa
        stateMachine.AddTransition(EnemyWanderState, EnemyChaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        stateMachine.AddTransition(EnemyChaseState, EnemyWanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));

        stateMachine.SetState(EnemyWanderState);
    }

    void Update() 
    {
        stateMachine.Update();
    }

    void FixedUpdate() 
    {
        stateMachine.FixedUpdate();
    }
}