using UnityEngine;
using SaiUtils.StateMachine;
using Sirenix.OdinInspector;

public class EnemyBrain : MonoBehaviour 
{
    [Header("Enemy Components")]
    [SerializeField] private Rigidbody enemyRigidbody;
    public Rigidbody EnemyRigidbody => enemyRigidbody;
    [SerializeField] private PlayerDetector playerDetector;
    public PlayerDetector PlayerDetector => playerDetector;

    [Header("Wander Settings")]
    
    [Tooltip("This is the ground layer")]
    [SerializeField] private LayerMask groundLayerMask;
    
    [Tooltip("Speed at which the enemy wanders")]
    [SerializeField] private float wanderSpeed = 2f;
    
    [Tooltip("Radius within which the enemy wanders")]
    [SerializeField] private float wanderRadius = 5f;
    
    [Tooltip("Distance to check for pits or edges")]
    [SerializeField] private float pitCheckDistance = 5f;

    [Tooltip("Time range for how long the enemy wanders before changing direction")]       
    [SerializeField] private Vector2 wanderTimeRange = new(1f, 3f);

    [Header("Chase Settings")]
    [Tooltip("Time the enemy remembers the player after losing sight")]
    [SerializeField] private float playerRememberTime = 5f;

    [Tooltip("Speed at which the enemy chases the player")]
    [SerializeField] private float chaseSpeed = 5f;


    [Header("Attack Settings")]
    [SerializeField] private float attackDistance = 2f;

    [Header("Debug")]
    [SerializeField, ReadOnly] private float _facingDirection;
    public float FacingDirection => _facingDirection;

    private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    public EnemyWanderState EnemyWanderState { get; private set; }
    public EnemyChaseState EnemyChaseState { get; private set; }
    public EnemyAttackState EnemyAttackState { get; private set; }
    public EnemyFleeState EnemyFleeState { get; private set; }

    private void Awake() {
        stateMachine = new StateMachine();
        EnemyWanderState = new EnemyWanderState(this, stateMachine, enemyRigidbody, FacingDirection, groundLayerMask, wanderTimeRange, wanderSpeed, wanderRadius, pitCheckDistance);
        EnemyChaseState = new EnemyChaseState(this, stateMachine, enemyRigidbody, playerRememberTime, chaseSpeed, groundLayerMask, pitCheckDistance);
        EnemyAttackState = new EnemyAttackState(this, stateMachine, enemyRigidbody);
        EnemyFleeState = new EnemyFleeState(this, stateMachine, enemyRigidbody);

        // Wander State to Chase State and vice versa
        stateMachine.AddTransition(EnemyWanderState, EnemyChaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        stateMachine.AddAnyTransition(EnemyWanderState, new BlankPredicate());

        stateMachine.SetState(EnemyWanderState);
    }

    private void Update() 
    {
        stateMachine.Update();
    }

    private void FixedUpdate() 
    {
        stateMachine.FixedUpdate();
    }
}