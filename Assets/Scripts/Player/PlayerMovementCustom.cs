using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerMovementCustom : MonoBehaviour
{
    [SerializeField] private CustomMovementData _data;
    [SerializeField] private Transform _art;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private bool _debug;

    [Header("Collisions")]
    [SerializeField] private LayerMask _groundLayer = 1;
    [SerializeField] private Collider _groundCollider;
    [SerializeField] private float _groundCheckOffset = 0.8f;
    [SerializeField] private Vector2 _groundCheckSize = new Vector3(0.5f, 0.05f, 0.5f);
    [SerializeField] private Collider _ceilingCollider;
    [SerializeField] private float _ceilingCheckOffset = 0.8f;
    [SerializeField] private Vector2 _ceilingCheckSize = new Vector3(0.5f, 0.15f, 0.5f);
    [SerializeField] private Collider _leftWallCollider;
    [SerializeField] private Collider _rightWallCollider;
    [SerializeField] private float _wallCheckOffset = 0.5f;
    [SerializeField] private Vector2 _wallCheckSize = new Vector3(0.3f, 1f, 0.5f);

    [Header("Feedback")]
    [SerializeField] private SfxReference _jumpSfx;
    [SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private SfxReference _landSfx;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _walkParticles;
    [SerializeField] private SfxReference _dashSfx;
    [SerializeField] private ParticleSystem _dashParticles;

    [Header("Debug - Movement")]
    [SerializeField, ReadOnly] private float _moveHorz;
    [SerializeField, ReadOnly] private float _moveVert;
    [SerializeField, ReadOnly] private float _gravityScale;

    [Header("Debug - Collisions")]
    [SerializeField, ReadOnly] private bool _groundCollided;
    [SerializeField, ReadOnly] private float _coyoteGrounded;
    [SerializeField, ReadOnly] private bool _ceilingCollided;
    [SerializeField, ReadOnly] private bool _leftWallCollided;
    [SerializeField, ReadOnly] private float _coyoteLeftWall;
    [SerializeField, ReadOnly] private bool _rightWallCollided;
    [SerializeField, ReadOnly] private float _coyoteRightWall;

    [Header("Debug - Jump")]
    [SerializeField, ReadOnly] private bool _jumpInput;
    [SerializeField, ReadOnly] private float _jumpTimer;
    [SerializeField, ReadOnly] private bool _isJumping;
    [SerializeField, ReadOnly] private bool _isWallJumping;
    [SerializeField, ReadOnly] private float _wallJumpTimer;
    [SerializeField, ReadOnly] private bool _lastWallJumpRightDir;
    [SerializeField, ReadOnly] private bool _isJumpFalling;

    [Header("Debug - Dash")]
    [SerializeField, ReadOnly] private bool _dashInput;
    [SerializeField, ReadOnly] private int _dashesLeft;
    [SerializeField, ReadOnly] private float _dashTimer;
    [SerializeField, ReadOnly] private float _dashDelayTimer;
    [SerializeField, ReadOnly] private bool _isDashAttacking;
    [SerializeField, ReadOnly] private bool _isDashing;
    [SerializeField, ReadOnly] private float _postDashTimer;

    [Header("Debug - Other")]
    [SerializeField, ReadOnly] private bool _climbInput;
    [SerializeField, ReadOnly] private bool _climbing;
    [SerializeField, ReadOnly] private bool _sliding;
    [SerializeField, ReadOnly] private bool _facingRight;

    private Coroutine _dashRoutine;

    private void Start()
    {
        CheckCollisions(true);
        _rb.useGravity = false;
    }

    private void Update()
    {
        CheckFacingDir();
        CheckCollisions();
        UpdateTimers(Time.deltaTime);
        CheckPlayerInput();
        CheckJump();
        CheckDash();
        CheckClimbing();
        CheckSliding();
        CheckGravity();
    }

    private void FixedUpdate()
    {
        Run();
        Climb();
        // Gravity
        _rb.AddForce(Vector3.up * (Physics.gravity.y * _gravityScale), ForceMode.Acceleration);
    }

    private void SetGravityScale(float gravityScale)
    {
        _gravityScale = gravityScale;
    }

    private void CheckFacingDir()
    {
        bool facingRight = _rb.velocity.x > 0;
        if (_facingRight != facingRight)
        {
            _facingRight = !_facingRight;
            Vector3 scale = _art.localScale;
            scale.x *= -1;
            _art.localScale = scale;
        }
    }

    private void CheckCollisions(bool force = false)
    {
        var pos = transform.position;
        bool grounded = Physics.CheckBox(pos + Vector3.down * _groundCheckOffset, _groundCheckSize, transform.rotation, _groundLayer);
        bool ceiling = Physics.CheckBox(pos + Vector3.up * _ceilingCheckOffset, _ceilingCheckSize, transform.rotation, _groundLayer);
        bool leftWall = Physics.CheckBox(pos + Vector3.left * _wallCheckOffset, _wallCheckSize, transform.rotation, _groundLayer);
        bool rightWall = Physics.CheckBox(pos + Vector3.right * _wallCheckOffset, _wallCheckSize, transform.rotation, _groundLayer);

        if (grounded != _groundCollided || force)
        {
            _groundCollided = grounded;
            if (_groundCollider != null) _groundCollider.enabled = grounded;
            if (grounded)
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
            if (_ceilingCollider != null) _ceilingCollider.enabled = ceiling;
        }
        if (leftWall != _leftWallCollided || force)
        {
            _leftWallCollided = leftWall;
            if (_leftWallCollider != null) _leftWallCollider.enabled = leftWall;
        }
        if (rightWall != _rightWallCollided || force)
        {
            _rightWallCollided = rightWall;
            if (_rightWallCollider != null) _rightWallCollider.enabled = rightWall;
        }
    }

    private void UpdateTimers(float deltaTime)
    {
        if (_groundCollided) _coyoteGrounded = _data.coyoteTime;
        else if (_coyoteGrounded > 0) _coyoteGrounded -= deltaTime;
        else _coyoteGrounded = 0;

        if (_leftWallCollided) _coyoteLeftWall = _data.coyoteTime;
        else if (_coyoteLeftWall > 0) _coyoteLeftWall -= deltaTime;
        else _coyoteLeftWall = 0;

        if (_rightWallCollided) _coyoteRightWall = _data.coyoteTime;
        else if (_coyoteRightWall > 0) _coyoteRightWall -= deltaTime;
        else _coyoteRightWall = 0;

        if (_jumpTimer > 0) _jumpTimer -= deltaTime;
        else _jumpTimer = 0;

        if (_wallJumpTimer > 0) _wallJumpTimer -= deltaTime;
        else _wallJumpTimer = 0;

        if (_dashTimer > 0) _dashTimer -= deltaTime;
        else _dashTimer = 0;

        if (_dashDelayTimer > 0) _dashDelayTimer -= deltaTime;
        else _dashDelayTimer = 0;

        if (_postDashTimer > 0) _postDashTimer -= deltaTime;
        else _postDashTimer = 0;
    }

    private void CheckPlayerInput()
    {
        _moveHorz = Input.GetAxisRaw("Horizontal");
        _moveVert = Input.GetAxisRaw("Vertical");

        _climbInput = Input.GetButton("Climb");

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
    }

    private void CheckJump()
    {
        if (_isJumping && _rb.velocity.y < 0)
        {
            _isJumping = false;
            if (!_isWallJumping) _isJumpFalling = true;
        }

        if (_isWallJumping && _wallJumpTimer == 0)
        {
            _isWallJumping = false;
        }

        if (_coyoteGrounded > 0 && !_isJumping && !_isWallJumping)
        {
            _isJumpFalling = false;
        }

        if (_jumpTimer > 0 && !_isDashAttacking) AttemptJump();
        //else if (InputManager.StartJumpThisFrame && (_isDashing || _postDashTimer > 0)) AttemptJump();
    }

    private void AttemptJump()
    {
        if (!_isJumping && _coyoteGrounded > 0)
        {
            Jump();
            return;
        }

        bool leftWall = _coyoteLeftWall > 0;
        bool rightWall = _coyoteRightWall > 0;
        if (!_isWallJumping && (leftWall || rightWall))
        {
            if (leftWall && rightWall) WallJump(!_lastWallJumpRightDir);
            else WallJump(rightWall);
        }
    }

    private void Jump()
    {
        Log("Jump");

        ResetAllJumpValues();
        _isJumping = true;

        float force = _data.jumpForce;
        if (_rb.velocity.y < 0) force -= _rb.velocity.y;
        _rb.AddForce(Vector2.up * force, ForceMode.Impulse);

        if (_isDashing || _postDashTimer > 0)
        {
            if (_rb.velocity.x < 0) _rb.AddForce(Vector2.left * (force * 1f), ForceMode.Impulse);
            if (_rb.velocity.x > 0) _rb.AddForce(Vector2.right * (force * 1f), ForceMode.Impulse);
            Log("Dash Boost");
        }

        StopDashing();
    }

    private void WallJump(bool rightWall)
    {
        Log("Wall Jump");

        ResetAllJumpValues();
        _isWallJumping = true;
        _wallJumpTimer = _data.wallJumpTime;
        _lastWallJumpRightDir = rightWall;

        Vector2 force = new Vector2(_data.wallJumpForce.x, _data.wallJumpForce.y);
        force.x *= rightWall ? -1 : 1;
        if (Mathf.Sign(_rb.velocity.x) != Mathf.Sign(force.x)) force.x -= _rb.velocity.x;

        if (_rb.velocity.y < 0) force.y -= _rb.velocity.y;
        _rb.AddForce(force, ForceMode.Impulse);

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
        _isWallJumping = false;
        _isJumpFalling = false;
        _jumpTimer = 0;
        _groundCollided = false;
        _groundCollider.enabled = false;
        _coyoteGrounded = 0;
        _leftWallCollided = false;
        _coyoteLeftWall = 0;
        _rightWallCollided = false;
        _coyoteRightWall = 0;
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
            _rb.velocity = dir * _data.dashSpeed;
            yield return null;
        }
        _isDashAttacking = false;
        _coyoteGrounded = 0;

        SetGravityScale(_data.gravityScale);
        _rb.velocity = _data.dashEndSpeed * dir;

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

    private void CheckClimbing()
    {
        if (_isJumping || _isWallJumping || _isDashing)
        {
            _climbing = false;
            return;
        }
        bool hugLeft = _coyoteLeftWall > 0 && _moveHorz <= 0;
        bool hugRight = _coyoteRightWall > 0 && _moveHorz >= 0;
        _climbing = _climbInput && (hugLeft || hugRight);
    }

    private void CheckSliding()
    {
        if (_isJumping || _isWallJumping || _isDashing || _climbing)
        {
            _sliding = false;
            return;
        }
        bool hugLeft = _coyoteLeftWall > 0 && (_sliding || _rb.velocity.x < 0);// && InputManager.MoveDir.x < 0;
        bool hugRight = _coyoteRightWall > 0 && (_sliding || _rb.velocity.x > 0);// && InputManager.MoveDir.x > 0;
        _sliding = hugLeft || hugRight;
    }

    private void CheckGravity()
    {
        if (_climbing || _isDashAttacking) SetGravityScale(0);
        else if (_sliding)
        {
            SetGravityScale(_data.gravityScale * _data.slideGravityMult);
        }
        else if (_rb.velocity.y > 0 && _moveVert < 0)
        {
            SetGravityScale(_data.gravityScale * _data.fastFallGravityMult);
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -_data.maxFastFallSpeed));
        }
        else if ((_isJumping || _isWallJumping || _isJumpFalling) && Mathf.Abs(_rb.velocity.y) < _data.jumpHangTimeThreshold)
        {
            SetGravityScale(_data.gravityScale * _data.jumpHangGravityMult);
        }
        else if (_rb.velocity.y < 0)
        {
            SetGravityScale(_data.gravityScale * _data.fallGravityMult);
            _rb.velocity = new Vector2(_rb.velocity.x, Mathf.Max(_rb.velocity.y, -_data.maxFallSpeed));
        }
        else
        {
            SetGravityScale(_data.gravityScale);
        }
    }

    private void Run()
    {
        if (_isDashAttacking || _climbing) return;

        float lerpAmount = 1;
        if (_isWallJumping) lerpAmount = _data.wallJumpRunLerp;
        else if (_isDashing) lerpAmount = _data.dashEndRunLerp;

        float targetSpeed = _moveHorz * _data.runMaxSpeed;
        targetSpeed = Mathf.Lerp(_rb.velocity.x, targetSpeed, lerpAmount);

        float accelRate;
        if (_groundCollided) accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _data.runAccelAmount : _data.runDeccelAmount;
        else accelRate = Mathf.Abs(targetSpeed) > 0.01f ? _data.runAccelAmount * _data.accelInAir : _data.runDeccelAmount * _data.deccelInAir;

        if ((_isJumping || _isWallJumping || _isJumpFalling) && Mathf.Abs((_rb.velocity.y)) < _data.jumpHangTimeThreshold)
        {
            accelRate *= _data.jumpHangAccelerationMult;
            targetSpeed *= _data.jumpHangMaxSpeedMult;
        }

        if (_data.doConserveMomentum && Mathf.Abs(_rb.velocity.x) > Mathf.Abs(targetSpeed) &&
            Mathf.Sign(_rb.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && !_groundCollided)
        {
            accelRate = 0;
        }

        float speedDif = targetSpeed - _rb.velocity.x;
        float movement = speedDif * accelRate;

        _rb.AddForce(movement * Vector2.right, ForceMode.Force);
    }

    private void Climb()
    {
        if (!_climbing) return;

        float targetSpeed = _moveVert * _data.climbSpeed;

        float speedDif = targetSpeed - _rb.velocity.y;
        float movement = speedDif * _data.climbAccel;

        float maxValue = Mathf.Abs(speedDif) * 1f / Time.fixedDeltaTime;
        movement = Mathf.Clamp(movement, -maxValue, maxValue);

        _rb.AddForce(movement * Vector2.up, ForceMode.Impulse);

        var dir = _coyoteRightWall > 0 ? Vector2.right : Vector2.left;
        _rb.AddForce(dir, ForceMode.Impulse);
    }

    private void OnDrawGizmos()
    {
        var pos = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos + Vector3.down * _groundCheckOffset, _groundCheckSize);
        Gizmos.DrawWireCube(pos + Vector3.up * _ceilingCheckOffset, _ceilingCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos + Vector3.left * _wallCheckOffset, _wallCheckSize);
        Gizmos.DrawWireCube(pos + Vector3.right * _wallCheckOffset, _wallCheckSize);
    }

    private void Log(string message)
    {
        if (_debug) Debug.Log("Action: " + message, gameObject);
    }
}
