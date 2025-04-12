using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerMovementCustom : MonoBehaviour
{
    [SerializeField] private CustomMovementData _data;
    [SerializeField] private Transform _art;
    [SerializeField] private bool _debug;
    [SerializeField] private float _jetpackMaxFuel = 1;

    [Header("Collisions")]
    [SerializeField] private LayerMask _groundLayer = 1;
    [SerializeField] private float _playerHeight = 1.6f;
    [SerializeField] private float _playerWidth = 1.0f;
    [SerializeField] private float _groundOffset = 0.8f;
    [SerializeField] private float _groundCheckWidth = 0.3f;
    [SerializeField] private float _ceilingCheckWidth = 0.3f;
    [SerializeField] private float _wallCheckWidth = 0.5f;
    [SerializeField] private float _wallChecVerticalOffset = 0.25f;

    [Header("Feedback")]
    [SerializeField] private SfxReference _jumpSfx;
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private SfxReference _landSfx;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _walkParticles;
    [SerializeField] private SfxReference _dashSfx;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private ParticleSystem _jetpackParticles;

    [Header("Debug - Movement")]
    [SerializeField, ReadOnly] private Vector3 _velocity;
    [SerializeField, ReadOnly] private bool _facingRight;
    [SerializeField, ReadOnly] private float _moveHorz;
    [SerializeField, ReadOnly] private float _moveVert;
    [SerializeField, ReadOnly] private float _gravityScale;

    [Header("Debug - Collisions")]
    [SerializeField, ReadOnly] private bool _groundCollided;
    [SerializeField, ReadOnly] private float _groundDistance;
    [SerializeField, ReadOnly] private float _coyoteGrounded;
    [SerializeField, ReadOnly] private bool _ceilingCollided;
    [SerializeField, ReadOnly] private float _ceilingDistance;
    [SerializeField, ReadOnly] private bool _leftWallCollided;
    [SerializeField, ReadOnly] private float _leftWallDistance;
    [SerializeField, ReadOnly] private bool _rightWallCollided;
    [SerializeField, ReadOnly] private float _rightWallDistance;

    [Header("Debug - Jump")]
    [SerializeField, ReadOnly] private bool _jumpInput;
    [SerializeField, ReadOnly] private float _jumpTimer;
    [SerializeField, ReadOnly] private bool _isJumping;
    [SerializeField, ReadOnly] private bool _isJumpFalling;

    [Header("Debug - Dash")]
    [SerializeField, ReadOnly] private bool _dashInput;
    [SerializeField, ReadOnly] private int _dashesLeft;
    [SerializeField, ReadOnly] private float _dashTimer;
    [SerializeField, ReadOnly] private float _dashDelayTimer;
    [SerializeField, ReadOnly] private bool _isDashAttacking;
    [SerializeField, ReadOnly] private bool _isDashing;
    [SerializeField, ReadOnly] private float _postDashTimer;

    [Header("Debug - Jetpack")]
    [SerializeField, ReadOnly] private bool _jetpackInput;
    [SerializeField, ReadOnly] private bool _jetpackActive;
    [SerializeField, ReadOnly] private float _jetpackFuel;
    [SerializeField, ReadOnly] private float _jetpackRefuelTimer;

    private Coroutine _dashRoutine;

    private void Start()
    {
        CheckCollisions(true);
    }

    private void Update()
    {
        CheckCollisions();
        UpdateTimers(Time.deltaTime);
        CheckPlayerInput();
        CheckJump();
        CheckDash();
        CheckJetpack();
        CheckGravity();
        Run(Time.deltaTime);
        CheckFacingDir();

        // Gravity
        _velocity += Vector3.up * (Physics.gravity.y * _gravityScale * Time.deltaTime);

        if (_ceilingCollided && _velocity.y > 0)
        {
            _velocity.y = 0;
            _isJumping = false;
        }
        _velocity.y = Mathf.Clamp(_velocity.y, -20, 20);
        if (_groundCollided && _velocity.y < 0) _velocity.y = 0;
        Vector3 offset = _velocity * Time.deltaTime;
        offset.y = Mathf.Clamp(offset.y, -_groundDistance, _ceilingDistance);
        offset.x = Mathf.Clamp(offset.x, -_leftWallDistance, _rightWallDistance);
        transform.position += offset;
        if ((_leftWallCollided && _velocity.x < 0) || (_rightWallCollided && _velocity.x > 0)) _velocity.x = 0;
    }

    private void SetGravityScale(float gravityScale)
    {
        _gravityScale = gravityScale;
    }

    private void CheckFacingDir()
    {
        if (_velocity.x == 0) return;
        bool facingRight = _velocity.x > 0;
        if (_facingRight != facingRight)
        {
            _facingRight = facingRight;
            Vector3 scale = _art.localScale;
            scale.x *= -1;
            _art.localScale = scale;
        }
    }

    private void CheckCollisions(bool force = false)
    {
        var pos = transform.position;
        RaycastHit hitInfo;

        _groundDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++)
        {
            if (Physics.Raycast(pos + Vector3.right * _groundCheckWidth * i, Vector3.down, out hitInfo, _groundOffset + 2, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _groundOffset;
                if (dist < _groundDistance) _groundDistance = dist;
            }
        }
        bool grounded = _groundDistance < 0.05f;

        _ceilingDistance = Mathf.Infinity;
        for (int i = -1; i <= 1; i++)
        {
            if (Physics.Raycast(pos + Vector3.up * _ceilingCheckWidth * i, Vector3.up, out hitInfo, (_playerHeight - _groundOffset) + 2, _groundLayer, QueryTriggerInteraction.Ignore))
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
            if (Physics.Raycast(wallPos, Vector3.left, out hitInfo, _playerWidth * 0.5f + 2, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _playerWidth * 0.5f;
                if (dist < _leftWallDistance) _leftWallDistance = dist;
            }
            if (Physics.Raycast(wallPos, Vector3.right, out hitInfo, _playerWidth * 0.5f + 2, _groundLayer, QueryTriggerInteraction.Ignore))
            {
                var dist = hitInfo.distance - _playerWidth * 0.5f;
                if (dist < _rightWallDistance) _rightWallDistance = dist;
            }
        }
        bool leftWall = _leftWallDistance < 0.05f;
        bool rightWall = _rightWallDistance < 0.05f;

        if (grounded != _groundCollided || force)
        {
            _groundCollided = grounded;
            if (_groundCollided)
            {
                _landSfx.Play();
                _landParticles.Play();
                _walkParticles.Play();
            }
            else
            {
                _walkParticles.Stop();
            }
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

    private void UpdateTimers(float deltaTime)
    {
        if (_groundCollided) _coyoteGrounded = _data.coyoteTime;
        else if (_coyoteGrounded > 0) _coyoteGrounded -= deltaTime;
        else _coyoteGrounded = 0;

        if (_jumpTimer > 0) _jumpTimer -= deltaTime;
        else _jumpTimer = 0;

        if (_dashTimer > 0) _dashTimer -= deltaTime;
        else _dashTimer = 0;

        if (_dashDelayTimer > 0) _dashDelayTimer -= deltaTime;
        else _dashDelayTimer = 0;

        if (_postDashTimer > 0) _postDashTimer -= deltaTime;
        else _postDashTimer = 0;

        if (_jetpackActive)
        {
            _jetpackRefuelTimer = _data.jetpackRegenDelay;
            _jetpackFuel -= _data.jetpackConsumeRate * deltaTime;
        }
        else if (_jetpackRefuelTimer > 0) _jetpackRefuelTimer -= deltaTime;

        if (!_jetpackActive && _jetpackFuel < _jetpackMaxFuel && _jetpackRefuelTimer <= 0f)
        {
            _jetpackFuel += _data.jetpackRegenRate * deltaTime;
            _jetpackFuel = Mathf.Min(_jetpackFuel, _jetpackMaxFuel);
        }
    }

    private void CheckPlayerInput()
    {
        _moveHorz = Input.GetAxisRaw("Horizontal");
        _moveVert = Input.GetAxisRaw("Vertical");

        var jumpInput = Input.GetButton("Jump");
        if (jumpInput != _jumpInput)
        {
            _jumpInput = jumpInput;
            if (jumpInput) _jumpTimer = _data.jumpInputBufferTime;
        }
        var dashInput = Input.GetButton("Dash");
        if (dashInput != _dashInput)
        {
            _dashInput = dashInput;
            if (dashInput) _dashTimer = _data.dashInputBufferTime;
        }
        _jetpackInput = Input.GetButton("Jump");
    }

    private void CheckJump()
    {
        if (_isJumping && _velocity.y < 0)
        {
            _isJumping = false;
            _isJumpFalling = true;
        }

        if (_coyoteGrounded > 0 && !_isJumping)
        {
            _isJumpFalling = false;
        }

        if (_jumpTimer > 0 && !_isDashAttacking) AttemptJump();
        //else if (InputManager.StartJumpThisFrame && (_isDashing || _postDashTimer > 0)) AttemptJump();
    }

    private void AttemptJump()
    {
        if (!_isJumping && (_groundCollided || _coyoteGrounded > 0))
        {
            Jump();
            return;
        }
    }

    private void Jump()
    {
        Log("Jump");

        ResetAllJumpValues();

        _isJumping = true;
        _velocity.y = _data.jumpForce;

        if (_isDashing || _postDashTimer > 0)
        {
            if (_velocity.x < 0) _velocity += Vector3.left * _data.jumpForce;
            if (_velocity.x > 0) _velocity += Vector3.right * _data.jumpForce;
            Log("Dash Boost");
        }

        JumpFeedback();

        StopDashing();
    }

    private void JumpFeedback(bool particles = true)
    {
        _jumpSfx.Play();
        if (particles) _jumpParticles.Play();
    }

    private void StopDashing()
    {
        if (_dashRoutine != null)
        {
            StopCoroutine(_dashRoutine);
            _isDashAttacking = false;
            _isDashing = false;
            _postDashTimer = 0;
        }
    }

    private void ResetAllJumpValues()
    {
        _isJumping = false;
        _isJumpFalling = false;
        _jumpTimer = 0;
        _groundCollided = false;
        _walkParticles.Stop();
        _coyoteGrounded = 0;
        _leftWallCollided = false;
        _rightWallCollided = false;
    }

    private void CheckDash()
    {
        if (_groundCollided && _dashDelayTimer == 0) _dashesLeft = _data.dashAmount;

        if (_dashTimer > 0 && _dashesLeft > 0 && _dashDelayTimer == 0)
        {
            Dash();
        }
    }

    private void Dash()
    {
        Log("Dash");

        _dashDelayTimer = _data.dashRefillTime;

        var dir = new Vector2(_moveHorz, _moveVert);
        if (dir == Vector2.zero) dir = _facingRight ? Vector2.right : Vector2.left;
        dir = dir.normalized;

        ResetAllJumpValues();
        _dashTimer = 0;

        _isDashing = true;
        _dashesLeft--;

        if (_dashRoutine != null) StopCoroutine(_dashRoutine);
        _dashRoutine = StartCoroutine(StartDash(dir));
    }

    private IEnumerator StartDash(Vector2 dir)
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(_data.dashSleepTime);
        Time.timeScale = 1;

        SetGravityScale(0);

        _dashSfx.Play();
        _dashParticles.Play();

        _isDashAttacking = true;
        float startTime = Time.time;
        while (Time.time - startTime <= _data.dashAttackTime)
        {
            _velocity = dir * _data.dashSpeed;
            yield return null;
        }
        _isDashAttacking = false;
        _coyoteGrounded = 0;

        SetGravityScale(_data.gravityScale);
        _velocity = _data.dashEndSpeed * dir;

        startTime = Time.time;
        while (Time.time - startTime <= _data.dashEndTime)
        {
            yield return null;
        }

        _dashParticles.Stop();

        _isDashing = false;
        _postDashTimer = _data.dashJumpBonusBufferTime;
        _dashRoutine = null;
    }

    private void CheckJetpack()
    {
        var active = _jetpackInput && !_isJumping && !_isDashAttacking && _jetpackFuel > 0;
        if (_jetpackActive != active)
        {
            _jetpackActive = active;
            if (_jetpackActive) _jetpackParticles.Play();
            else _jetpackParticles.Stop();
        }
    }

    private void CheckGravity()
    {
        if (_jetpackActive)
        {
            SetGravityScale(-_data.jetpackForce);
        }
        else if (_isDashAttacking)
        {
            SetGravityScale(0);
        }
        else if (_velocity.y > 0 && _moveVert < 0)
        {
            SetGravityScale(_data.gravityScale * _data.fastFallGravityMult);
            _velocity = new Vector3(_velocity.x, Mathf.Max(_velocity.y, -_data.maxFastFallSpeed), 0f);
        }
        else if (_jumpInput && _isJumping)
        {
            SetGravityScale(_data.gravityScale * _data.jumpHangGravityMult);
        }
        else if ((_isJumping || _isJumpFalling) && Mathf.Abs(_velocity.y) < _data.jumpHangTimeThreshold)
        {
            SetGravityScale(_data.gravityScale * _data.jumpHangGravityMult * 4);
        }
        else if (_velocity.y < 0)
        {
            SetGravityScale(_data.gravityScale * _data.fallGravityMult);
            _velocity = new Vector3(_velocity.x, Mathf.Max(_velocity.y, -_data.maxFallSpeed), 0f);
        }
        else
        {
            SetGravityScale(_data.gravityScale);
        }
    }

    private void Run(float deltaTime)
    {
        if (_isDashAttacking) return;

        float lerpAmount = 1;
        if (_isDashing) lerpAmount = _data.dashEndRunLerp;

        float targetSpeed = _moveHorz * _data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(_velocity.x, targetSpeed, lerpAmount);

        float accelRate;
        if (_groundCollided) accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _data.runAccelAmount : _data.runDeccelAmount;
        else accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _data.runAccelAmount * _data.accelInAir : _data.runDeccelAmount * _data.deccelInAir;

        if ((_isJumping || _isJumpFalling) && Mathf.Abs((_velocity.y)) < _data.jumpHangTimeThreshold)
        {
            accelRate *= _data.jumpHangAccelerationMult;
            targetSpeed *= _data.jumpHangMaxSpeedMult;
        }

        if (_data.doConserveMomentum && Mathf.Abs(_velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(_velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && !_groundCollided)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - _velocity.x;
        float movement = speedDif * accelRate;

        _velocity += Vector3.right * movement * deltaTime;
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

    private void Log(string message)
    {
        if (_debug) Debug.Log("Action: " + message, gameObject);
    }
}
