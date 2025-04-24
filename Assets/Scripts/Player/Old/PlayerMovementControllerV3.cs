using System;
using Sirenix.OdinInspector;
using UnityEngine;

public struct FrameInput
{
    public Vector2 MoveInput;
    public bool JumpDown;
    public bool JumpHeld;
}

public class PlayerMovementControllerV3 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PCStatsV3 stats; 
    [SerializeField] private Transform _groundCheck; 
    [SerializeField] private Transform _ceilingCheck; 
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Collider _collider;

    [Header("General Debug")]
    [SerializeField, ReadOnly] private FrameInput _frameInput;
    public FrameInput FrameInput => _frameInput;
    [SerializeField, ReadOnly] private Vector3 _frameVelocity;
    [SerializeField, ReadOnly] private float _time;

    [Header("Collision Debug")]
    [SerializeField, ReadOnly] private float _frameLeftGrounded = float.MinValue;
    [SerializeField, ReadOnly] private bool _grounded;

    [Header("Jump Debug")]
    [SerializeField, ReadOnly] private bool _jumpToConsume;
    [SerializeField, ReadOnly] private bool _bufferedJumpUseable;
    [SerializeField, ReadOnly] private bool _endedJumpEarly;
    [SerializeField, ReadOnly] private bool _coyoteUseable;
    [SerializeField, ReadOnly] private float _timeJumpWasPressed;

    private bool HasBufferedJump => _bufferedJumpUseable && _time < _timeJumpWasPressed + stats.JumpBuffer;
    private bool HasCoyoteJump => _coyoteUseable && !_grounded && _time < _frameLeftGrounded + stats.CoyoteTime;
    public event Action<bool> OnGroundedChanged;
    public event Action Jumped;


    private void Awake() 
    {
        _rb = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _rb.freezeRotation = true;

    }

    private void Update()
    {
        _time += Time.deltaTime;
        GatherInput();
    }

    private void GatherInput()
    {
        _frameInput = new FrameInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump"),
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"))
        };

        if (stats.SnapInput)
        {
            _frameInput.MoveInput.x = Mathf.Abs(_frameInput.MoveInput.x) < stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.MoveInput.x);
            _frameInput.MoveInput.y = Mathf.Abs(_frameInput.MoveInput.y) < stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.MoveInput.y);
        }

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        HandleJump();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void CheckCollisions()
    {
        RaycastHit hitGround;
        bool groundHit = Physics.SphereCast(_groundCheck.position, _collider.bounds.extents.x, Vector3.down, out hitGround, stats.GrounderDistance, stats.PlayerLayer);
        
        RaycastHit hitCeiling;
        bool ceilingHit = Physics.SphereCast(_ceilingCheck.position, _collider.bounds.extents.x, Vector3.up, out hitCeiling, stats.GrounderDistance, stats.PlayerLayer);

        if (ceilingHit) _frameVelocity.y = Mathf.Min(_frameVelocity.y, 0); 

        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUseable = true;
            _bufferedJumpUseable = true;
            _endedJumpEarly = false;
            OnGroundedChanged?.Invoke(true);
        }
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = _time;
            OnGroundedChanged?.Invoke(false);
        }
    }

    private void HandleJump()
    {
        if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;
        if (!_jumpToConsume && !HasBufferedJump) return;
        if (_grounded || HasCoyoteJump) ExecuteJump();
        _jumpToConsume = false;
    }

    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUseable = false;
        _coyoteUseable = false;
        _frameVelocity.y = stats.JumpPower;
        Jumped?.Invoke();
    }

    private void HandleDirection()
    {
        if (_frameInput.MoveInput.x == 0)
        {
            var deceleration = _grounded ? stats.GroundDeceleration : stats.AirDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
        }
        else
        {
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.MoveInput.x * stats.MaxSpeed, stats.Acceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _frameVelocity.y <= 0f)
        {
            _frameVelocity.y = stats.GroundingForce;
        }
        else
        {
            var inAirGravity = stats.FallAcceleration;
            if (_endedJumpEarly && _frameVelocity.y > 0f) inAirGravity *= stats.JumpEndEarlyGravityModifier;
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement() => _rb.velocity = new Vector3(_frameVelocity.x, _frameVelocity.y, 0f);
}