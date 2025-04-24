using UnityEngine;
using Sirenix.OdinInspector;

public class EntityMovement : MonoBehaviour
{
    [Header("Collision Settings")]
    [SerializeField] protected float _gravityScale = 1;
    [SerializeField] protected LayerMask _groundLayer = 1;
    [SerializeField] protected float _playerHeight = 1.4f;
    [SerializeField] protected float _playerWidth = 1.0f;
    [SerializeField] protected float _groundOffset = 0.7f;
    [SerializeField] protected float _groundCheckWidth = 0.4f;
    [SerializeField] protected float _ceilingCheckWidth = 0.25f;
    [SerializeField] protected float _wallCheckWidth = 0.5f;
    [SerializeField] protected float _wallChecVerticalOffset = 0.15f;
    [SerializeField] protected float _checkRaycastDistance = 5f;

    [Header("Debug - Movement")]
    [SerializeField, ReadOnly] protected Vector3 _velocity;
    [SerializeField, ReadOnly] protected bool _groundCollided;
    [SerializeField, ReadOnly] protected float _groundDistance;
    [SerializeField, ReadOnly] protected bool _ceilingCollided;
    [SerializeField, ReadOnly] protected float _ceilingDistance;
    [SerializeField, ReadOnly] protected bool _leftWallCollided;
    [SerializeField, ReadOnly] protected float _leftWallDistance;
    [SerializeField, ReadOnly] protected bool _rightWallCollided;
    [SerializeField, ReadOnly] protected float _rightWallDistance;

    private void Start()
    {
        CheckCollisions(true);
    }

    protected virtual void Update()
    {
        CheckCollisions();
        ApplyGravity();
        ApplyVelocity();
    }

    protected void CheckCollisions(bool force = false)
    {
        var pos = transform.position;
        RaycastHit hitInfo;

        _groundDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++)
        {
            if (Physics.Raycast(pos + Vector3.right * _groundCheckWidth * i, Vector3.down, out hitInfo, _groundOffset + _checkRaycastDistance, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _groundOffset;
                if (dist < _groundDistance) _groundDistance = dist;
            }
        }
        bool grounded = _groundDistance < 0.05f;

        _ceilingDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++)
        {
            if (Physics.Raycast(pos + Vector3.right * _ceilingCheckWidth * i, Vector3.up, out hitInfo, (_playerHeight - _groundOffset) + _checkRaycastDistance, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - (_playerHeight - _groundOffset);
                if (dist < _ceilingDistance) _ceilingDistance = dist;
            }
        }
        bool ceiling = _ceilingDistance < 0.05f;

        _leftWallDistance = Mathf.Infinity;
        _rightWallDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++)
        {
            var wallPos = pos + Vector3.up * (_wallChecVerticalOffset + _wallCheckWidth * i);
            if (Physics.Raycast(wallPos, Vector3.left, out hitInfo, _playerWidth * 0.5f + _checkRaycastDistance, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _playerWidth * 0.5f;
                if (dist < _leftWallDistance) _leftWallDistance = dist;
            }
            if (Physics.Raycast(wallPos, Vector3.right, out hitInfo, _playerWidth * 0.5f + _checkRaycastDistance, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _playerWidth * 0.5f;
                if (dist < _rightWallDistance) _rightWallDistance = dist;
            }
        }
        bool leftWall = _leftWallDistance < 0.05f;
        bool rightWall = _rightWallDistance < 0.05f;

        if (grounded != _groundCollided || force)
        {
            UpdateGrounded(grounded);
        }
        if (ceiling != _ceilingCollided || force)
        {
            _ceilingCollided = ceiling;
        }
        if (leftWall != _leftWallCollided || force)
        {
            _leftWallCollided = leftWall;
        }
        if (rightWall != _rightWallCollided || force)
        {
            _rightWallCollided = rightWall;
        }
    }

    protected virtual void UpdateGrounded(bool grounded)
    {
        _groundCollided = grounded;
    }

    protected void SetGravityScale(float gravityScale)
    {
        _gravityScale = gravityScale;
    }

    protected void ApplyGravity()
    {
        _velocity += Vector3.up * (Physics.gravity.y * _gravityScale * Time.deltaTime);
    }

    protected void ApplyVelocity()
    {
        _velocity.y = Mathf.Clamp(_velocity.y, -20, 20);
        if (_groundCollided && _velocity.y < 0) _velocity.y = 0;
        Vector3 offset = _velocity * Time.deltaTime;
        offset.y = Mathf.Clamp(offset.y, -_groundDistance, _ceilingDistance);
        offset.x = Mathf.Clamp(offset.x, -_leftWallDistance, _rightWallDistance);
        transform.position += offset;
        if ((_leftWallCollided && _velocity.x < 0) || (_rightWallCollided && _velocity.x > 0)) _velocity.x = 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var pos = transform.position;
        Gizmos.color = Color.red;
        for (int i = -1; i <= 1; i++)
        {
            Gizmos.DrawRay(pos + Vector3.down * _groundOffset + Vector3.right * _groundCheckWidth * i, Vector3.down * 0.1f);
            Gizmos.DrawRay(pos + Vector3.up * (_playerHeight - _groundOffset) + Vector3.right * _ceilingCheckWidth * i, Vector3.up * 0.1f);
            Gizmos.DrawRay(pos + Vector3.left * _playerWidth * 0.5f + Vector3.up * (_wallChecVerticalOffset + _wallCheckWidth * i), Vector3.left * 0.1f);
            Gizmos.DrawRay(pos + Vector3.right * _playerWidth * 0.5f + Vector3.up * (_wallChecVerticalOffset + _wallCheckWidth * i), Vector3.right * 0.1f);
        }
        pos += Vector3.up * (_playerHeight * 0.5f - _groundOffset);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(pos, new Vector3(_playerWidth, _playerHeight, 0f));
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(pos, _velocity);
    }
#endif
}
