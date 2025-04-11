using System;
using Sirenix.OdinInspector;
using UnityEngine;

// Structure to hold raycast origin points in 3D
public struct RaycastOrigins3D
{
    public Vector3 TopLeft; 
    public Vector3 TopRight;
    public Vector3 BottomLeft;
    public Vector3 BottomRight;
}

public class PlayerControllerV2 : MonoBehaviour
{
    [SerializeField] PCStats _stats;
    [SerializeField] LayerMask _collisionMask;
    [Header("3D Collider Settings")]
    [SerializeField] Vector3 _colliderSize = new Vector3(1f, 2f, 1f);
    [SerializeField] Vector3 _colliderCenterOffset = Vector3.zero;
    [SerializeField] float _skinWidth = 0.015f; 
    [SerializeField] int _horizontalRayCount = 4;
    [SerializeField] int _verticalRayCount = 4;

    [Header("Debug")]
    [SerializeField, ReadOnly] Vector3 _velocity;
    [SerializeField, ReadOnly] Vector2 _input;
    [SerializeField, ReadOnly] bool _grounded;
    [SerializeField, ReadOnly] bool _wasGroundedLastFrame;
    [SerializeField, ReadOnly] bool _jumpHeld;
    [SerializeField, ReadOnly] bool _jumpPressed;
    [SerializeField, ReadOnly] bool _jumpReleased;
    [SerializeField, ReadOnly] Vector3 _position; 
    float _initialZPosition; 

    float _horizontalRaySpacing;
    float _verticalRaySpacing;
    BoxCollider _collider; 
    RaycastOrigins3D _raycastOrigins; 

    public Vector3 Velocity => _velocity; 
    public Vector2 Input => _input;
    public bool IsGrounded => _grounded;

    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    [SerializeField, ReadOnly] float _coyoteTimer;
    [SerializeField, ReadOnly] float _jumpBufferTimer;

    void Awake()
    {
        _position = transform.position;
        _initialZPosition = _position.z;

        _collider = GetComponent<BoxCollider>();
        if (_collider == null)
        {
            Debug.LogWarning("PlayerControllerV2 expects a BoxCollider component. Attempting to use serialized size/offset.");
            // If using serialized size without a collider component, ensure _colliderSize and _colliderCenterOffset are accurate.
        }
        else
        {
            // _colliderSize = _collider.size;
            // _colliderCenterOffset = _collider.center;
        }
        CalculateRaySpacing();
    }

    void Update()
    {
        _wasGroundedLastFrame = _grounded;
        _position = transform.position;
        _position.z = _initialZPosition;

        // 1. Calculate Ray Origins based on current position
        UpdateRaycastOrigins();
        // 2. Check collisions (especially ground) BEFORE calculating movement logic
        CheckCollisions();

        // 3. Gather Input
        GatherInput();

        // 4. Handle Timers (Coyote/Buffer) - Needs up-to-date ground info
        HandleTimers();

        // 5. Handle Jump Logic
        HandleJump();
        HandleJumpCut();

        // 6. Apply Gravity (Only affects Y axis)
        ApplyGravity();

        // 7. Calculate Horizontal Velocity (Only affects X axis)
        CalculateHorizontalVelocity();

        // 8. Move and Resolve Collisions (using Vector3 velocity)
        MoveAndSlide(_velocity * Time.deltaTime);

        // 9. Update Transform Position (Constraining Z)
        _position.z = _initialZPosition; 
        transform.position = _position;

        // 10. Trigger Grounded Changed Event if state changed
        if (_grounded != _wasGroundedLastFrame)
        {
            GroundedChanged?.Invoke(_grounded, 1f); 
            if (_grounded) _coyoteTimer = _stats.CoyoteTime;
        }
    }

    void GatherInput()
    {
        float x = UnityEngine.Input.GetAxisRaw("Horizontal");
        // float y = UnityEngine.Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(x) < _stats.HorizontalDeadZoneThreshold) x = 0;

        if (_stats.SnapInput && x != 0)
        {
            x = Mathf.Sign(x);
        }

        _input = new Vector2(x, 0); 
        // _input = new Vector2(x, y); 

        _jumpPressed = UnityEngine.Input.GetButtonDown("Jump");
        _jumpHeld = UnityEngine.Input.GetButton("Jump");
        _jumpReleased = UnityEngine.Input.GetButtonUp("Jump");
    }

    void HandleTimers()
    {
        if (!_grounded && _wasGroundedLastFrame)
        {
            _coyoteTimer = _stats.CoyoteTime;
        }
        else if (_coyoteTimer > 0)
        {
            _coyoteTimer -= Time.deltaTime;
        }


        if (_jumpPressed)
        {
            _jumpBufferTimer = _stats.JumpBuffer;
        }
        else if (_jumpBufferTimer > 0)
        {
            _jumpBufferTimer -= Time.deltaTime;
        }

        _coyoteTimer = Mathf.Max(0, _coyoteTimer);
        _jumpBufferTimer = Mathf.Max(0, _jumpBufferTimer);
    }

    void HandleJump()
    {
        if (_jumpBufferTimer > 0)
        {
            if (_coyoteTimer > 0 || _grounded)
            {
                PerformJump();
                _jumpBufferTimer = 0;
                _coyoteTimer = 0;
            }
        }
    }

    void PerformJump()
    {
        _velocity.y = _stats.JumpPower;
        _grounded = false;
        Jumped?.Invoke();
        _wasGroundedLastFrame = false;
    }

    void HandleJumpCut()
    {
         if (_jumpReleased && _velocity.y > 0)
         {
            _velocity.y *= 0.5f;
         }

         // Apply stronger gravity if falling OR if jump button released early
         // Note: Using _stats.Gravity directly here, ensure PCStats has appropriate 3D gravity value
         if (_velocity.y < 0 || (_velocity.y > 0 && !_jumpHeld))
         {
            // This modifier should ideally be controlled by PCStats
            float gravityMultiplier = _stats.JumpEndEarlyGravityModifier > 0 ? _stats.JumpEndEarlyGravityModifier : 1f;
            _velocity.y -= _stats.Gravity * gravityMultiplier * Time.deltaTime;
         }
    }

    void ApplyGravity()
    {
        if (!_grounded)
        {
             if (!(_velocity.y > 0 && !_jumpHeld))
             {
                _velocity.y -= _stats.Gravity * Time.deltaTime;
             }
        }
        else if (_velocity.y <= 0)
        {
            _velocity.y = -_stats.StickToGroundForce;
        }

        _velocity.y = Mathf.Max(_velocity.y, -_stats.MaxFallSpeed);
    }

    void CalculateHorizontalVelocity()
    {
        float targetSpeed = _input.x * _stats.MaxSpeed;
        float acceleration = _grounded ? _stats.Acceleration : _stats.AirAcceleration;
        float deceleration = _grounded ? _stats.GroundDeceleration : _stats.AirDeceleration;

        if (_input.x != 0) // Accelerating
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, targetSpeed, acceleration * Time.deltaTime);
        }
        else // Decelerating
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, 0, deceleration * Time.deltaTime);
        }
        // Clamp X velocity
        _velocity.x = Mathf.Clamp(_velocity.x, -_stats.MaxSpeed, _stats.MaxSpeed);
         // Ensure Z velocity is always zero
        _velocity.z = 0;
    }


    Bounds GetColliderBounds()
    {
        Vector3 center = _position + _colliderCenterOffset;
        return new Bounds(center, _colliderSize);
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = GetColliderBounds();
        bounds.Expand(_skinWidth * -2);

        _raycastOrigins.BottomLeft = new Vector3(bounds.min.x, bounds.min.y, bounds.center.z);
        _raycastOrigins.BottomRight = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);
        _raycastOrigins.TopLeft = new Vector3(bounds.min.x, bounds.max.y, bounds.center.z);
        _raycastOrigins.TopRight = new Vector3(bounds.max.x, bounds.max.y, bounds.center.z);
        // Using bounds.center.z assumes rays cast from the middle depth.
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = GetColliderBounds();
        bounds.Expand(_skinWidth * -2);

        _horizontalRayCount = Mathf.Max(2, _horizontalRayCount);
        _verticalRayCount = Mathf.Max(2, _verticalRayCount);

        _horizontalRaySpacing = bounds.size.y / (_horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (_verticalRayCount - 1);
    }

    void CheckCollisions()
    {
        _grounded = false;
        float rayLength = _skinWidth + 0.5f; 

        for (int i = 0; i < _verticalRayCount; i++)
        {
            Vector3 rayOrigin = _raycastOrigins.BottomLeft + Vector3.right * (_verticalRaySpacing * i);

            bool hitDetected = Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, rayLength, _collisionMask);

            Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);

            if (hitDetected)
            {
                _grounded = true;
                
                _position.y = hit.point.y + _skinWidth;
                if (_velocity.y < 0)
                {
                    _velocity.y = -_stats.StickToGroundForce; 
                }

                break;
            }
        }
    }


    void MoveAndSlide(Vector3 moveAmount) 
    {
        UpdateRaycastOrigins(); 

        if (Mathf.Abs(moveAmount.x) > float.Epsilon)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            float rayLength = Mathf.Abs(moveAmount.x) + _skinWidth;

            for (int i = 0; i < _horizontalRayCount; i++)
            {
                Vector3 rayOrigin = (directionX == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
                rayOrigin += Vector3.up * (_horizontalRaySpacing * i);
                RaycastHit hit;

                if (Physics.Raycast(rayOrigin, Vector3.right * directionX, out hit, rayLength, _collisionMask))
                {
                    // Adjust X move amount
                    moveAmount.x = (hit.distance - _skinWidth) * directionX;
                    rayLength = hit.distance; // Prevent hitting further objects
                    _velocity.x = 0; // Stop horizontal velocity
                }
                 Debug.DrawRay(rayOrigin, Vector3.right * directionX * rayLength, Color.blue);
            }
        }
        _position.x += moveAmount.x; 


        if (Mathf.Abs(moveAmount.y) > float.Epsilon)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = Mathf.Abs(moveAmount.y) + _skinWidth;

            for (int i = 0; i < _verticalRayCount; i++)
            {
                Vector3 rayOrigin = (directionY == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
                rayOrigin += Vector3.right * (_verticalRaySpacing * i + moveAmount.x);
                RaycastHit hit;

                if (Physics.Raycast(rayOrigin, Vector3.up * directionY, out hit, rayLength, _collisionMask))
                {
                    moveAmount.y = (hit.distance - _skinWidth) * directionY;
                    rayLength = hit.distance; // Prevent hitting further objects

                    if (directionY > 0 || (directionY < 0 && _velocity.y < 0))
                    {
                        _velocity.y = 0;
                    }
                    if (directionY < 0)
                    {
                        _grounded = true;
                    }
                }
                 Debug.DrawRay(rayOrigin, Vector3.up * directionY * rayLength, Color.green);
            }
        }
        _position.y += moveAmount.y; 

    }

    void OnDrawGizmos()
    {
        Vector3 currentPosition = Application.isPlaying ? _position : transform.position;
        currentPosition.z = Application.isPlaying ? _initialZPosition : transform.position.z; 


        Gizmos.color = Color.yellow;
        Vector3 boundsCenter = currentPosition + _colliderCenterOffset;
        Gizmos.DrawWireCube(boundsCenter, _colliderSize);


        // Need to calculate spacing based on non-playing state if needed for editor drawing
        // but UpdateRaycastOrigins relies on playing state _position. It's complex to fully replicate outside play mode.
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        UpdateRaycastOrigins(); 
        Gizmos.DrawSphere(_raycastOrigins.BottomLeft, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.BottomRight, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.TopLeft, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.TopRight, 0.05f);
    }
}