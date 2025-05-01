using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerMovementCustom : EntityMovement
{
    [Header("Player Settings")]
    [SerializeField] private CustomMovementData _data;
    [SerializeField] private Transform _art;
    [SerializeField] private bool _debug;
    [SerializeField] private float _jetpackMaxFuel = 1;

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
    [SerializeField, ReadOnly] private float _moveHorz;
    [SerializeField, ReadOnly] private float _moveVert;
    [SerializeField, ReadOnly] private bool _facingLeft;

    [Header("Debug - Jump")]
    [SerializeField, ReadOnly] private bool _jumpInput;
    [SerializeField, ReadOnly] private float _jumpTimer;
    [SerializeField, ReadOnly] private bool _isJumping;
    [SerializeField, ReadOnly] private bool _isJumpFalling;
    [SerializeField, ReadOnly] private float _coyoteGrounded;

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

    protected override void Update()
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

        ApplyGravity();
        if (_ceilingCollided && _velocity.y > 0)
        {
            _velocity.y = 0;
            _isJumping = false;
        }
        ApplyVelocity();
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
        _moveHorz = PlayerInput.PlayerMoveHorz;
        _moveVert = PlayerInput.PlayerMoveVert;

        var jumpInput = PlayerInput.PlayerJump;
        if (jumpInput != _jumpInput)
        {
            _jumpInput = jumpInput;
            if (jumpInput) _jumpTimer = _data.jumpInputBufferTime;
        }
        var dashInput = PlayerInput.PlayerDash;
        if (dashInput != _dashInput)
        {
            _dashInput = dashInput;
            if (dashInput) _dashTimer = _data.dashInputBufferTime;
        }
        _jetpackInput = PlayerInput.PlayerJetpack;
    }

    protected override void UpdateGrounded(bool grounded)
    {
        base.UpdateGrounded(grounded);
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

#region Jump

    private void CheckJump()
    {
        if (_isJumping && _velocity.y < 2)
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

#endregion
#region Dash

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
        if (dir == Vector2.zero) dir = _facingLeft ? Vector2.right : Vector2.left;
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

    #endregion

    private void CheckJetpack()
    {
        var active = _jetpackInput && !_isJumping && !_isDashAttacking && _jetpackFuel > 0;
        if (_jetpackActive != active)
        {
            _jetpackActive = active;

            if (_jetpackParticles) 
            {
                if (_jetpackActive) _jetpackParticles.Play();
                else _jetpackParticles.Stop();
            }
        }
    }

    private void CheckGravity()
    {
        if (_jetpackActive)
        {
            // TODO: Reduce force as the player approaches _data.jetpackMaxVelocity
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

    private void CheckFacingDir()
    {
        var mousePosition = PlayerInput.MousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector3 directionToMouse = worldMousePosition - transform.position;
        bool facingLeft = directionToMouse.x < 0;

        if (_facingLeft != facingLeft)
        {
            _facingLeft = facingLeft;
            Vector3 scale = _art.localScale;
            scale.x = Mathf.Abs(scale.x) * (_facingLeft ? -1 : 1);
            _art.localScale = scale;
        }
    }

    private void Log(string message)
    {
        if (_debug) Debug.Log("Action: " + message, gameObject);
    }
}
