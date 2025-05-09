using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using SaiUtils.ImprovedTimers;

public class EnemyMovement : EntityMovement 
{
    
    [Header("Enemy References")]
    [SerializeField] PlayerDetector _playerDetector;

    [Header("Enemy Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _jumpForce = 10f;
    [SerializeField] float _strafeSpeedMultiplier = 0.75f;

    [Header("Debug - Enemy")]
    [SerializeField] bool _enableDebug = false;
    [SerializeField, ReadOnly] private bool _facingRight = true;
    [SerializeField, ReadOnly] private bool _wallAhead = false;
    [SerializeField, ReadOnly] private bool _pitAhead = false;

    private EnemyBrain _enemyBrain;

    CountdownTimer flipCooldownTimer = new CountdownTimer(2f); // 0.5 seconds cooldown for flipping

    void Awake() 
    {
        _enemyBrain = GetComponent<EnemyBrain>();
        if (_enemyBrain == null) 
        {
            //Debug.LogError("EnemyMovement requires an EnemyBrain component.");
        }
    }


    // ---- Functions To Be Called By State Machine ----
    // returns true if possible or false if blocked by obstacle
    [Button("Patrol")]
    public bool Patrol()
    {
        if (!_enemyBrain.CanMove) return false;
        CheckObstacleAhead();
        if (_wallAhead || _pitAhead)
        {
            

            var rand = Random.Range(0f, 1f);
            if (rand < 0.07f && _enemyBrain.CanJump) // 50% chance to jump
            {
                Jump();
            }
            else
            {
                StopMoving();
                Flip();
            }
        }

        _velocity.x = _moveSpeed * (_facingRight ? 1 : -1);
        return true;
    }

    [Button("Chase Target")]
    public bool ChaseTarget(Transform _target)
    {
        if (!_enemyBrain.CanMove) return false;

        if (_target == null) 
        {
            if (_enableDebug && Player.Instance != null) _target = Player.Instance.transform;
            else { _velocity.x = 0f; return false; }
        }

        var directionToPlayer = Mathf.Sign(_target.position.x - transform.position.x);
        if (directionToPlayer != 0 && directionToPlayer != (_facingRight ? 1 : -1))
        {
            Flip();
        }

        if (_wallAhead && !_pitAhead && _groundCollided && _enemyBrain.CanJump) // If wall but no pit, maybe jump?
        {
            _velocity.x = _moveSpeed * (_facingRight ? 1 : -1); 
            Jump(); 
            return true; 
        }
        else if (_wallAhead || _pitAhead) // If pit or unjumpable wall
        {
            _velocity.x = 0f; 
            return false; 
        }
        

        _velocity.x = _moveSpeed * (_facingRight ? 1 : -1);
        return true;
    }

    [Button("Strafe")]
    public void Strafe(float _direction)
    {
        if (!_enemyBrain.CanStrafe) return;
        _velocity.x = _moveSpeed * _strafeSpeedMultiplier * _direction;
    }

    public bool MoveTowards(Vector3 _targetPos)
    {
        if (!_enemyBrain.CanMove) return false;
        if (_targetPos == null) 
        {
            if (_enableDebug && Player.Instance != null) _targetPos = Player.Instance.transform.position;
            else { _velocity.x = 0f; return false; }
        }

        var directionToPlayer = Mathf.Sign(_targetPos.x - transform.position.x);
        if (directionToPlayer != 0 && directionToPlayer != (_facingRight ? 1 : -1))
        {
            Flip();
        }

        CheckObstacleAhead();
        if (_wallAhead || _pitAhead)
        {
            _velocity.x = 0f; 
            return false; 
        }

        _velocity.x = _moveSpeed * (_facingRight ? 1 : -1);
        if (Mathf.Abs(transform.position.x - _targetPos.x) < 0.5f) {
            _velocity.x = 0f;
            return true; // Reached destination (horizontally)
        }
        
        return true;
    }

   

    [Button("Retreat")]
    public bool Retreat(Vector3 _lastSeenPlayerPos)
    {
        if (!_enemyBrain.CanMove) return false;
        if (_lastSeenPlayerPos == null) 
        {
            if (_enableDebug)
            {
                _lastSeenPlayerPos = Player.Instance.transform.position;
            }
            else
            {
                _velocity.x = 0f;
                return false; 
            }
        }

        var directionToPlayer = Mathf.Sign(_lastSeenPlayerPos.x - transform.position.x);
        if ((directionToPlayer > 0 && !_facingRight) || (directionToPlayer < 0 && _facingRight))
        {
            Flip();
        }

        CheckObstacleAhead();
        if (_wallAhead || _pitAhead)
        {
            _velocity.x = 0f;
            return false; 
        }

        _velocity.x = -_moveSpeed * (_facingRight ? 1 : -1);
        return true;
    }

    [Button("Jump")]
    public void Jump()
    {
        if (_groundCollided)
        {
            _velocity.y = _jumpForce;
        }
    }

    [Button("Stop Moving")]
    public void StopMoving()
    {
        _velocity.x = 0f;
    }

    public void EnsureFacingTarget(Transform target)
    {
        if (target == null) return;
        var directionToTarget = Mathf.Sign(target.position.x - transform.position.x);
        if (directionToTarget != 0 && directionToTarget != (_facingRight ? 1 : -1))
        {
            Flip();
        }
    }

    public void SetHorizontalVelocity(float velocity)
    {
        if (!_enemyBrain.CanMove) return;
        _velocity.x = velocity;
    }

    // ----- Private Functions ----
    void CheckObstacleAhead()
    {
        // Original version which relies on PlayerDetector more
        //_wallAhead = Physics.Raycast(transform.position, Vector3.right * (_facingRight ? 1 : -1), _obstacleCheckDistance, _playerDetector.ObstacleLayer);
        //_pitAhead = Physics.Raycast(transform.position, Vector3.down, _pitFallCheckDistance, _playerDetector.ObstacleLayer);
        
        
        // Alternative/Simpler Wall Check: Use base class collision flags
        //_wallAhead = (_facingRight && _rightWallCollided) || (!_facingRight && _leftWallCollided);
    
        
        if (_facingRight && RightWallCollided) _wallAhead = true;
        else if (!_facingRight && LeftWallCollided) _wallAhead = true;
        else _wallAhead = false;

        if (!LeftBottomCollided || !RightBottomCollided) _pitAhead = true;
        else _pitAhead = false;
    
    }

    [Button("Flip")]
    private void Flip()
    {
        if (flipCooldownTimer.IsRunning) return;
        flipCooldownTimer.Start(); 
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
        
        // Vector3 scaler = transform.localScale;
        // scaler.x *= -1;
        // transform.localScale = scaler;
    }

   
}