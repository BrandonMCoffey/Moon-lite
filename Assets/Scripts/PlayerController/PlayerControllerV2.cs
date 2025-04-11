using System;
using Sirenix.OdinInspector;
using UnityEngine;

// Structure to hold raycast origin points
public struct RaycastOrigins
{
    public Vector2 TopLeft, TopRight;
    public Vector2 BottomLeft, BottomRight;
}

public class PlayerControllerV2 : MonoBehaviour
{
    [SerializeField] PCStats _stats;
    [SerializeField] LayerMask _collisionMask;
    [SerializeField] Vector2 _colliderSize = new Vector2(0.5f, 1.5f); // Treat this as half-extents if using center below
    [SerializeField] float _skinWidth = 0.01f; // Reduced slightly, often helps with precision
    [SerializeField] int _horizontalRayCount = 4;
    [SerializeField] int _verticalRayCount = 4;

    [Header("Debug")]
    [SerializeField, ReadOnly] Vector2 _velocity;
    [SerializeField, ReadOnly] Vector2 _input;
    [SerializeField, ReadOnly] bool _grounded;
    [SerializeField, ReadOnly] bool _wasGroundedLastFrame; 
    [SerializeField, ReadOnly] bool _jumpHeld;
    [SerializeField, ReadOnly] bool _jumpPressed;
    [SerializeField, ReadOnly] bool _jumpReleased;
    [SerializeField, ReadOnly] Vector2 _position; 

    // Pre-calculate ray spacing
    float _horizontalRaySpacing;
    float _verticalRaySpacing;
    BoxCollider2D _collider; // Optional: Use a BoxCollider2D for bounds reference
    RaycastOrigins _raycastOrigins;

    public Vector2 Velocity => _velocity; 
    public Vector2 Input => _input;
    public bool IsGrounded => _grounded; 

    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    [SerializeField, ReadOnly] float _coyoteTimer;
    [SerializeField, ReadOnly] float _jumpBufferTimer;

    void Awake()
    {
        _position = transform.position;
        _collider = GetComponent<BoxCollider2D>();
        if (_collider == null)
        {
            Debug.LogWarning("PlayerControllerV2 expects a BoxCollider2D component for bounds reference (though not strictly required for raycasting).");
        }
        CalculateRaySpacing();
    }

    void Update()
    {
        _wasGroundedLastFrame = _grounded;
        _position = transform.position;

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

        // 6. Apply Gravity
        ApplyGravity();

        // 7. Calculate Horizontal Velocity
        CalculateHorizontalVelocity(); // Renamed for clarity

        // 8. Move and Resolve Collisions
        MoveAndSlide(_velocity * Time.deltaTime);

        // 9. Update Transform Position
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
        float y = UnityEngine.Input.GetAxisRaw("Vertical");

        // Apply deadzone
        if (Mathf.Abs(x) < _stats.HorizontalDeadZoneThreshold) x = 0;
        if (Mathf.Abs(y) < _stats.VerticalDeadZoneThreshold) y = 0; // Only if using y input

      
        if (_stats.SnapInput && x != 0)
        {
            x = Mathf.Sign(x);
        }
        if (_stats.SnapInput && y != 0) y = Mathf.Sign(y); // Only if using y input

        _input = new Vector2(x, y);

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
            // You can either multiply or set to a smaller value
            // Setting might give more consistent short hop heights
            _velocity.y *= 0.5f; // Or _velocity.y = _velocity.y * (1 - _stats.JumpCutMultiplier);
            // Alternative: Apply stronger gravity instantly
            // This is handled by the gravity multiplier below now.
        }

        if (_velocity.y < 0 || (_velocity.y > 0 && !_jumpHeld))
        {
            _velocity.y -= _stats.Gravity * _stats.JumpEndEarlyGravityModifier * Time.deltaTime;
        }
    }


    void ApplyGravity()
    {
        // Apply standard gravity if not grounded or moving upwards
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
        _velocity.x = Mathf.Clamp(_velocity.x, -_stats.MaxSpeed, _stats.MaxSpeed);
    }


    Bounds GetColliderBounds()
    {
        // Assuming _position is the center of the player
        return new Bounds(_position, _colliderSize);


        // If _position is bottom-center, adjust calculation:
        // return new Bounds(_position + Vector2.up * _colliderSize.y * 0.5f, _colliderSize);
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = GetColliderBounds();
        // Apply skin width inset
        bounds.Expand(_skinWidth * -2);

        _raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        _raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        _raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        _raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = GetColliderBounds();
        // Apply skin width inset
        bounds.Expand(_skinWidth * -2);

        // Ensure at least 2 rays
        _horizontalRayCount = Mathf.Max(2, _horizontalRayCount);
        _verticalRayCount = Mathf.Max(2, _verticalRayCount);

        _horizontalRaySpacing = bounds.size.y / (_horizontalRayCount - 1);
        _verticalRaySpacing = bounds.size.x / (_verticalRayCount - 1);
    }

    void CheckCollisions()
    {
        _grounded = false; 
        float rayLength = _skinWidth + 0.1f; 

        for (int i = 0; i < _verticalRayCount; i++)
        {
            Vector2 rayOrigin = _raycastOrigins.BottomLeft + Vector2.right * (_verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayLength, _collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.down * rayLength, Color.red); 

            if (hit)
            {
                _grounded = true;
                
                _position.y = hit.point.y + _skinWidth;
                break; 
            }
        }
    }


    void MoveAndSlide(Vector2 moveAmount)
    {
        UpdateRaycastOrigins(); 

        if (Mathf.Abs(moveAmount.x) > float.Epsilon)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            float rayLength = Mathf.Abs(moveAmount.x) + _skinWidth;

            for (int i = 0; i < _horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.BottomRight;
                rayOrigin += Vector2.up * (_horizontalRaySpacing * i);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.blue); // Visualize rays

                if (hit)
                {
                    // --- Check for slope handling here if needed ---
                    // float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    moveAmount.x = (hit.distance - _skinWidth) * directionX;
                    rayLength = hit.distance; 
                    _velocity.x = 0;

                    // break; 
                    // Removed break to handle multiple hits potentially (though less common with adjusted rayLength)
                }
            }
        }
        _position.x += moveAmount.x; 


        if (Mathf.Abs(moveAmount.y) > float.Epsilon)
        {
            float directionY = Mathf.Sign(moveAmount.y);
            float rayLength = Mathf.Abs(moveAmount.y) + _skinWidth;

            for (int i = 0; i < _verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.BottomLeft : _raycastOrigins.TopLeft;
                rayOrigin += Vector2.right * (_verticalRaySpacing * i + moveAmount.x); // Adjust origin by horizontal move

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _collisionMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.green); // Visualize rays

                if (hit)
                {
                    moveAmount.y = (hit.distance - _skinWidth) * directionY;
                    rayLength = hit.distance; // Prevent hitting further objects
                    // Stop vertical velocity only if hitting something opposite to gravity or desired direction
                    // Ground collision stopping is handled by ApplyGravity setting velocity to StickForce
                    if (directionY > 0 || _velocity.y < 0) // Stop if moving up into ceiling OR moving down into ground
                    {
                        _velocity.y = 0;
                    }
                    if (directionY < 0)
                    {
                        _grounded = true; 
                    }
                }
            }
        }
         _position.y += moveAmount.y;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            _position = transform.position; 
            CalculateRaySpacing();
        }

        // Draw Collider Bounds
        Gizmos.color = Color.yellow;
        Bounds bounds = GetColliderBounds();
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        // Draw Ray Origins
        Gizmos.color = Color.cyan;
        UpdateRaycastOrigins();
        Gizmos.DrawSphere(_raycastOrigins.BottomLeft, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.BottomRight, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.TopLeft, 0.05f);
        Gizmos.DrawSphere(_raycastOrigins.TopRight, 0.05f);
    }
}
