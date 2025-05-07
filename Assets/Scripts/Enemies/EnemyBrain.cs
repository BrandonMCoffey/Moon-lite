using UnityEngine;
using SaiUtils.StateMachine;
using SaiUtils.GameEvents;
using Sirenix.OdinInspector;

public class EnemyBrain : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] EnemyMovement _enemyMovement;
    public EnemyMovement EnemyMovement => _enemyMovement;
    [SerializeField, ReadOnly] string _currentState;
    [SerializeField] PlayerDetector _playerDetector;
    public PlayerDetector PlayerDetector => _playerDetector;
    [SerializeField] EnemyWeaponManager _enemyWeaponManager;
    public EnemyWeaponManager EnemyWeaponManager => _enemyWeaponManager;
    [SerializeField] Health _health;
    public Health Health => _health;


    [Header("Attack State")]
    [SerializeField] bool _canAttack = true;
    public bool CanAttack => _canAttack;
    [SerializeField, ShowIf("_canAttack")] float _attackRange = 10f;
    public float AttackRange => _attackRange;
    [SerializeField, ShowIf("_canAttack")] float _preferredEngagementRange = 8f;
    public float PreferredEngagementRange => _preferredEngagementRange;
    [Range(0f, 1f)]
    [SerializeField, ShowIf("_canAttack")] float _strafeChance = 0.6f;
    public float StrafeChance => _strafeChance;
    [SerializeField, ShowIf("_canAttack")] float _repositionTimeMin = 3f; 
    public float RepositionTimeMin => _repositionTimeMin;
    [SerializeField, ShowIf("_canAttack")] float _repositionTimeMax = 6f; 
    public float RepositionTimeMax => _repositionTimeMax;

    [Header("Search State")]
    [SerializeField] bool _canSearch = true;
    public bool CanSearch => _canSearch;
    [SerializeField, ShowIf("_canSearch")] float _searchTime = 5f; // How long to search after losing sight
    public float SearchTime => _searchTime;

    [Header("Retreat State")]
    [SerializeField] bool _canRetreat = true;
    public bool CanRetreat => _canRetreat;
    [SerializeField, ShowIf("_canRetreat")] Vector2 _retreatDistanceRange = new Vector2(5f, 10f); 
    public float RetreatDistance => Random.Range(_retreatDistanceRange.x, _retreatDistanceRange.y); 
    [SerializeField, ShowIf("_canRetreat")] float _callForReinforcementsChance = 0.5f; 
    public float CallForReinforcementsChance => _callForReinforcementsChance;

    [Header("Blast-To State")]
    [SerializeField] bool _canBlastTo = true;
    public bool CanBlastTo => _canBlastTo;
    [SerializeField, ReadOnly, ShowIf("_canBlastTo")] Vector3 _blastToPosition;
    public Vector3 BlastToPosition => _blastToPosition;
    [SerializeField, ShowIf("_canBlastTo")] Vector2 _timeBlastingRange = new Vector2(1f, 3f);
    public float TimeBlasting => Random.Range(_timeBlastingRange.x, _timeBlastingRange.y); 

    [Header("Call For Reinforcement State")]
    [SerializeField] bool _canCallForReinforcement = true;
    public bool CanCallForReinforcement => _canCallForReinforcement;
    [SerializeField] bool _canHearCallForReinforcement = true;
    public bool CanHearCallForReinforcement => _canHearCallForReinforcement;
    [SerializeField, ReadOnly, ShowIf("_canHearCallForReinforcement")] bool _ignoreCallForReinforcement = false; 
    public bool IgnoreCallForReinforcement => _ignoreCallForReinforcement;
    [SerializeField, ShowIf("_canHearCallForReinforcement")] Vector2 _callForReinforcementAcceptRange = new Vector2(5f, 50f);
    [SerializeField, ShowIf("_canHearCallForReinforcement")] VectorTwoEvent _callForReinforcementEvent;
    public VectorTwoEvent CallForReinforcementEvent => _callForReinforcementEvent;

    StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyPickRetreatPosState PickRetreatPosState { get; private set; }
    public EnemyCallForReinforcementState CallForReinforcementState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemySearchState SearchState { get; private set; }
    public EnemyBlastToMoveState BlastToMoveState { get; private set; }
    public EnemyBlastToState BlastToState { get; private set; }

    void Awake() 
    {
        stateMachine = new StateMachine();
        PatrolState = new EnemyPatrolState(this, stateMachine);
        ChaseState = new EnemyChaseState(this, stateMachine);
        PickRetreatPosState = new EnemyPickRetreatPosState(this, stateMachine);
        AttackState = new EnemyAttackState(this, stateMachine);
        SearchState = new EnemySearchState(this, stateMachine);
        BlastToMoveState = new EnemyBlastToMoveState(this, stateMachine);
        BlastToState = new EnemyBlastToState(this, stateMachine);
        CallForReinforcementState = new EnemyCallForReinforcementState(this, stateMachine);

        // have each state manually take care of its own transitions
        stateMachine.AddAnyTransition(PatrolState, new BlankPredicate());
        stateMachine.AddAnyTransition(ChaseState, new BlankPredicate());
        stateMachine.AddAnyTransition(PickRetreatPosState, new BlankPredicate());
        stateMachine.AddAnyTransition(AttackState, new BlankPredicate());
        stateMachine.AddAnyTransition(SearchState, new BlankPredicate());
        stateMachine.AddAnyTransition(BlastToMoveState, new BlankPredicate());
        stateMachine.AddAnyTransition(BlastToState, new BlankPredicate());
        stateMachine.AddAnyTransition(CallForReinforcementState, new BlankPredicate());

        stateMachine.SetState(PatrolState);
    }

    private void OnEnable() => Health.OnHealthModified += OnHealthChanged;
    private void OnDisable() => Health.OnHealthModified -= OnHealthChanged;

    private void OnHealthChanged() 
    {
        if (Health.HealthPercentage <= 0.3)
        {
            stateMachine.ChangeState(PickRetreatPosState);
        }
    }

    public void SetCurrentStateText(string stateName) 
    {
        _currentState = stateName;
    }

    void Update() 
    {
        if (!MouseControllerManager.InGame) return;
        stateMachine.Update();
    }

    void FixedUpdate() 
    {
        stateMachine.FixedUpdate();
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _preferredEngagementRange);
    }

    public float GetDistanceFromPlayer()
    {
        if (PlayerDetector.PlayerTransform == null) return float.MaxValue;
        return Vector3.Distance(transform.position, PlayerDetector.PlayerTransform.position);
    }

    public void SetBlastToPosition(Vector3 position) 
    {
        _blastToPosition = position;
    }

    public void SetIgnoreCallForReinforcement(bool ignore) 
    {
        _ignoreCallForReinforcement = ignore;
    }

    public void ReactToCallForReinforcements(Vector2 position)
    {
        if (_ignoreCallForReinforcement) return;
        if (!_canHearCallForReinforcement) return;

        var callForReinforcementsVector3 = new Vector3(position.x, position.y, transform.position.z);
        var distanceToCallForReinforcements = Vector3.Distance(transform.position, callForReinforcementsVector3);
        
        if (distanceToCallForReinforcements < _callForReinforcementAcceptRange.x 
        || distanceToCallForReinforcements > _callForReinforcementAcceptRange.y) 
        {
            // checking if too close or too far
            return;
        }

        SetBlastToPosition(callForReinforcementsVector3);
        stateMachine.ChangeState(BlastToMoveState);
    }
}