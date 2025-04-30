using UnityEngine;
using SaiUtils.StateMachine;
using Sirenix.OdinInspector;

public class EnemyBrain : MonoBehaviour 
{
    [Header("References")]
    [SerializeField] EnemyMovement enemyMovement;
    [SerializeField] PlayerDetector playerDetector;

    StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyRetreatState RetreatState { get; private set; }

    void Awake() 
    {
        stateMachine = new StateMachine();
        PatrolState = new EnemyPatrolState(this, stateMachine, enemyMovement);
        ChaseState = new EnemyChaseState(this, playerDetector, stateMachine, enemyMovement);
        RetreatState = new EnemyRetreatState(this, playerDetector, stateMachine, enemyMovement);

        // have each state manually take care of its own transitions
        stateMachine.AddAnyTransition(PatrolState, new BlankPredicate());
        stateMachine.AddAnyTransition(ChaseState, new BlankPredicate());
        stateMachine.AddAnyTransition(RetreatState, new BlankPredicate());

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
}