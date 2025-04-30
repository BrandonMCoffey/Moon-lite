using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyBrain : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] EnemyMovement enemyMovement;
    public EnemyMovement EnemyMovement => enemyMovement;
    [SerializeField] PlayerDetector playerDetector;
    public PlayerDetector PlayerDetector => playerDetector;

    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    public float AttackRange => attackRange;

    StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyRetreatState RetreatState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    void Awake() 
    {
        stateMachine = new StateMachine();
        PatrolState = new EnemyPatrolState(this, stateMachine);
        ChaseState = new EnemyChaseState(this, stateMachine);
        RetreatState = new EnemyRetreatState(this, stateMachine);
        AttackState = new EnemyAttackState(this, stateMachine);
        

        // have each state manually take care of its own transitions
        stateMachine.AddAnyTransition(PatrolState, new BlankPredicate());
        stateMachine.AddAnyTransition(ChaseState, new BlankPredicate());
        stateMachine.AddAnyTransition(RetreatState, new BlankPredicate());
        stateMachine.AddAnyTransition(AttackState, new BlankPredicate());

        stateMachine.SetState(PatrolState);
    }

    void Update() 
    {
        stateMachine.Update();
    }

    void FixedUpdate() 
    {
        stateMachine.FixedUpdate();
    }

    private void OnDrawGizmosSelected() {
        // draw wire sphere for attack range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}