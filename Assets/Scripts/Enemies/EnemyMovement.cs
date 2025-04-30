using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class EnemyMovement : EntityMovement 
{
    
    [Header("Enemy References")]
    [SerializeField] PlayerDetector _playerDetector;

    [Header("Enemy Settings")]
    [SerializeField] float _moveSpeed = 5f;
    [SerializeField] float _jumpForce = 10f;

    [Header("Debug - Enemy")]
    [SerializeField] bool _enableDebug = false;
    [SerializeField, ReadOnly] private bool _facingRight = true;
    [SerializeField, ReadOnly] private bool _wallAhead = false;
    [SerializeField, ReadOnly] private bool _pitAhead = false;

    // ---- Functions To Be Called By State Machine ----
    
    // returns true if possible or false if blocked by obstacle
    [Button("Patrol")]
    public bool Patrol()
    {
        CheckObstacleAhead();
        if (_wallAhead || _pitAhead)
        {
            _velocity.x = 0f;
            return false; 
            // TODO: Add logic to turn around or change direction
        }

        _velocity.x = _moveSpeed * (_facingRight ? 1 : -1);
        return true;
    }

    [Button("Chase Target")]
    public bool ChaseTarget(Transform _target)
    {
        if (_target == null) 
        {
            if (_enableDebug)
            {
                _target = Player.Instance.transform;
            }
            else
            {
                _velocity.x = 0f;
                return false; 
            }
        }

        var directionToPlayer = Mathf.Sign(_target.position.x - transform.position.x);
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

        _velocity.x = _moveSpeed * (_facingRight ? 1 : -1);
        return true;
    }

    [Button("Retreat")]
    public bool Retreat(Vector3 _lastSeenPlayerPos)
    {
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
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
        
        // Vector3 scaler = transform.localScale;
        // scaler.x *= -1;
        // transform.localScale = scaler;
    }

   
}