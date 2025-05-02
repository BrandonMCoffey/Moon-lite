using UnityEngine;
using SaiUtils.StateMachine;

public class EnemyBrain : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] EnemyMovement enemyMovement;
    public EnemyMovement EnemyMovement => enemyMovement;
    [SerializeField] PlayerDetector playerDetector;
    public PlayerDetector PlayerDetector => playerDetector;
    [SerializeField] EnemyWeaponManager enemyWeaponManager;
    public EnemyWeaponManager EnemyWeaponManager => enemyWeaponManager;

    [Header("Settings")]
    [SerializeField] float attackRange = 10f;
    public float AttackRange => attackRange;

    [Header("AI Behavior")]
    [SerializeField, Range(0f, 1f)] float aggression = 0.5f;
    public float Aggression => aggression;
    [SerializeField] float preferredEngagementRange = 8f;
    public float PreferredEngagementRange => preferredEngagementRange;

    [Range(0f, 1f)]
    [SerializeField] float strafeChance = 0.6f;
    public float StrafeChance => strafeChance;

    [SerializeField] float repositionTimeMin = 3f; // Min time between repositions in combat
    public float RepositionTimeMin => repositionTimeMin;
    [SerializeField] float repositionTimeMax = 6f; // Max time between repositions in combat
    public float RepositionTimeMax => repositionTimeMax;

    [SerializeField] float searchTime = 5f; // How long to search after losing sight
    public float SearchTime => searchTime;

    StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyRetreatState RetreatState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemySearchState SearchState { get; private set; }

    void Awake() 
    {
        stateMachine = new StateMachine();
        PatrolState = new EnemyPatrolState(this, stateMachine);
        ChaseState = new EnemyChaseState(this, stateMachine);
        RetreatState = new EnemyRetreatState(this, stateMachine);
        AttackState = new EnemyAttackState(this, stateMachine);
        SearchState = new EnemySearchState(this, stateMachine);

        // have each state manually take care of its own transitions
        stateMachine.AddAnyTransition(PatrolState, new BlankPredicate());
        stateMachine.AddAnyTransition(ChaseState, new BlankPredicate());
        stateMachine.AddAnyTransition(RetreatState, new BlankPredicate());
        stateMachine.AddAnyTransition(AttackState, new BlankPredicate());
        stateMachine.AddAnyTransition(SearchState, new BlankPredicate());

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
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, preferredEngagementRange);
    }

    public float GetDistanceFromPlayer()
    {
        if (PlayerDetector.PlayerTransform == null) return float.MaxValue;
        return Vector3.Distance(transform.position, PlayerDetector.PlayerTransform.position);
    }
}