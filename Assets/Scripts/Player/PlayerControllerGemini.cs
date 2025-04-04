using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerControllerGemini : MonoBehaviour
{
    #region Components
    private Rigidbody rb;
    private CapsuleCollider coll;
    // Optional: Line Renderer for Grapple Hook visual
    private LineRenderer grappleLineRenderer;
    #endregion

    #region State Variables
    private bool isGrounded;
    private bool isDashing;
    private bool isGrappling;
    private bool isJetpacking;
    private bool wasGroundedLastFrame;
    private bool isKnockedBack;
    private bool canDash = true;
    private bool jumpBuffered;

    private Vector3 grapplePoint;
    private SpringJoint grappleJoint; // Alternative: Use physics joint for grapple
    #endregion

    #region Timers
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float dashTimer;
    private float dashCooldownTimer;
    private float knockbackTimer;
    #endregion

    #region Input Variables
    private float horizontalInput;
    private float verticalInput;
    private bool jumpInputDown;
    private bool jumpInputHeld;
    private bool dashInputDown;
    private bool grappleInputDown;
    private bool grappleInputUp;
    private bool jetpackInputHeld;
    #endregion

    #region Movement Parameters
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float airControlFactor = 0.5f; // How much control you have in the air
    [SerializeField] private float acceleration = 10f; // How quickly the player reaches max speed
    [SerializeField] private float deceleration = 15f; // How quickly the player stops
    #endregion

    #region Jumping Parameters
    [Header("Jumping")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float fallMultiplier = 2.5f; // Faster fall
    [SerializeField] private float lowJumpMultiplier = 2f; // Lower jump height if button released early
    [SerializeField] private float coyoteTimeDuration = 0.15f;
    [SerializeField] private float jumpBufferDuration = 0.1f;
    [SerializeField] private float maxFallSpeed = 40f; // Terminal velocity
    #endregion

    #region Dashing Parameters
    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 30f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float dashEndYMultiplier = 0.3f; // Retain some vertical speed after dash
    [SerializeField] private bool resetDashOnGround = true; // Can you only dash once per jump?
    #endregion

    #region Grapple Hook Parameters
    [Header("Grapple Hook")]
    [SerializeField] private float grappleRange = 20f;
    [SerializeField] private float grappleSpeed = 25f; // Speed player moves towards grapple point
    [SerializeField] private float grappleStopDistance = 1.5f; // How close to get before stopping grapple pull
    [SerializeField] private LayerMask grappleLayerMask; // What layers can be grappled onto?
    [SerializeField] private bool usePhysicsJointForGrapple = false; // Alternative grapple method
    [SerializeField] private float grappleJointSpring = 100f;
    [SerializeField] private float grappleJointDamper = 10f;
    #endregion

    #region Jetpack Parameters
    [Header("Jetpack")]
    [SerializeField] private float jetpackForce = 25f;
    [SerializeField] private float maxFuel = 1.0f;
    [SerializeField] private float currentFuel;
    [SerializeField] private float fuelConsumeRate = 0.5f; // Fuel units per second
    [SerializeField] private float fuelRegenRate = 0.3f; // Fuel units per second
    [SerializeField] private float fuelRegenDelay = 0.5f; // Delay after stopping use before regen starts
    private float fuelRegenTimer;
    #endregion

    #region Ground Check Parameters
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint; // Assign an empty GameObject slightly below the player's feet
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayerMask; // Set this to your "Ground" layer
    #endregion

    #region Knockback Parameters
    [Header("Knockback")]
    [SerializeField] private float knockbackResistance = 0.5f; // Higher means less knockback effect (0-1)
    #endregion

    #region Unity Methods

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();
        // Optional: Get LineRenderer if using it for grapple
        grappleLineRenderer = GetComponent<LineRenderer>();
        if (grappleLineRenderer) grappleLineRenderer.enabled = false;

        // Ensure Rigidbody settings are appropriate
        rb.freezeRotation = true; // Prevent unwanted tipping
    }

    void Start()
    {
        currentFuel = maxFuel; // Start with full fuel
    }

    void Update()
    {
        GatherInput();
        HandleTimers();
        CheckGrounded();
        HandleJumpBuffering();
        HandleCoyoteTime();
        HandleJetpackFuelRegen();
        TryGrapple();
        TryDash();

        // Handle state exit conditions based on input or timers
        if (dashTimer <= 0f && isDashing) StopDash();
        if (knockbackTimer <= 0f && isKnockedBack) StopKnockback();

        // Optional: Update grapple line visual
        if (isGrappling && grappleLineRenderer) UpdateGrappleLine();
    }

    void FixedUpdate()
    {
        // Only process movement if not being knocked back
        if (!isKnockedBack)
        {
            if (isDashing)
            {
                HandleDashMovement(); // Dashing overrides other movement
            }
            else if (isGrappling)
            {
                HandleGrappleMovement(); // Grappling overrides other movement
            }
            else
            {
                HandleMovement();
                HandleJumpingAndGravity();
                HandleJetpackMovement();
            }
        }

        LimitFallSpeed();
        wasGroundedLastFrame = isGrounded; // Store ground state for next frame's coyote time check
    }

    #endregion

    #region Input Handling

    void GatherInput()
    {
        horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down Arrow (for 3D movement)

        if (Input.GetButtonDown("Jump"))
        {
            jumpInputDown = true;
        }
        jumpInputHeld = Input.GetButton("Jump");

        dashInputDown = Input.GetKeyDown(KeyCode.LeftShift);   // Assign a button like Left Shift or Mouse Button
        grappleInputDown = Input.GetKeyDown(KeyCode.G); // Assign a button like Mouse Button 1 or E
        grappleInputUp = Input.GetKeyUp(KeyCode.G);

        jetpackInputHeld = Input.GetKey(KeyCode.J); // Assign a button (can be same as Jump)
    }

    #endregion

    #region State Checks

    void CheckGrounded()
    {
        // Use a sphere cast at the ground check point position
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayerMask);

        // Reset dash charges if configured to do so
        if (isGrounded && resetDashOnGround && !isDashing) // Make sure we aren't mid-dash
        {
            canDash = true;
        }
    }

    #endregion

    #region Timers

    void HandleTimers()
    {
        // Decrement timers if they are active
        if (coyoteTimeCounter > 0f) coyoteTimeCounter -= Time.deltaTime;
        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.deltaTime;
        if (dashTimer > 0f) dashTimer -= Time.deltaTime;
        if (dashCooldownTimer > 0f) dashCooldownTimer -= Time.deltaTime;
        if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;
        if (fuelRegenTimer > 0f) fuelRegenTimer -= Time.deltaTime;
    }

    #endregion

    #region Movement Logic

    void HandleMovement()
    {
        // Calculate move direction based on camera or world space
        // Assuming world space movement for simplicity in a Terraria-like cube world
        Vector3 targetDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        float currentSpeed = moveSpeed * (isGrounded ? 1f : airControlFactor); // Less control in air

        // Calculate target velocity
        Vector3 targetVelocity = targetDirection * currentSpeed;

        // --- Acceleration/Deceleration ---
        // Get current horizontal velocity
        Vector3 currentHorizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        // Calculate the desired change in velocity
        Vector3 velocityChange = targetVelocity - currentHorizontalVelocity;

        // Determine acceleration rate based on whether we are accelerating or decelerating
        float accelRate = (Vector3.Dot(targetVelocity, currentHorizontalVelocity) > 0 && targetVelocity.magnitude > 0.01f) ? acceleration : deceleration;

        // Apply acceleration, clamping the change to avoid overshooting
        Vector3 force = Vector3.ClampMagnitude(velocityChange, accelRate * Time.fixedDeltaTime);

        rb.AddForce(force, ForceMode.VelocityChange); // Use VelocityChange for snappier control

        // --- Direct Velocity Setting (Alternative - Simpler but less smooth) ---
        // float targetSpeedX = horizontalInput * currentSpeed;
        // float targetSpeedZ = verticalInput * currentSpeed;
        // Apply movement velocity, keeping existing Y velocity
        // rb.velocity = new Vector3(targetSpeedX, rb.velocity.y, targetSpeedZ);
    }

    #endregion

    #region Jumping & Gravity Logic

    void HandleCoyoteTime()
    {
        // If we were grounded last frame but are not now, start coyote timer
        if (wasGroundedLastFrame && !isGrounded && !isJetpacking && !isGrappling) // Don't activate if starting jetpack/grapple
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else if (isGrounded)
        {
            coyoteTimeCounter = 0f; // Reset if grounded
        }
    }

    void HandleJumpBuffering()
    {
        // If jump pressed but not allowed (e.g., in air without coyote time), start buffer
        if (jumpInputDown && !CanJump())
        {
            jumpBufferCounter = jumpBufferDuration;
        }
        // Reset buffer if jump button released
        if (!jumpInputHeld)
        {
            jumpBufferCounter = 0f;
        }
    }

    bool CanJump()
    {
        return (isGrounded || coyoteTimeCounter > 0f) && !isDashing && !isGrappling && !isKnockedBack;
    }

    bool ShouldJump()
    {
        // Actual jump trigger: Jump pressed now AND allowed, OR buffer is active AND we just landed
        bool jump = jumpInputDown;
        jumpInputDown = false;
        return (jump && CanJump()) || (jumpBufferCounter > 0f && isGrounded);
    }

    void HandleJumpingAndGravity()
    {
        // --- Jumping ---
        if (ShouldJump())
        {
            // Apply immediate upward velocity change
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Zero out Y velocity before jump
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            coyoteTimeCounter = 0f; // Consume coyote time
            jumpBufferCounter = 0f; // Consume jump buffer
        }

        // --- Variable Jump Height & Enhanced Gravity ---
        if (rb.velocity.y < 0) // If falling
        {
            // Apply stronger gravity when falling
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !jumpInputHeld && !isJetpacking) // If moving up and jump button released (and not jetpacking)
        {
            // Apply downward force for shorter jump
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (!isGrounded && !isJetpacking && !isGrappling && !isDashing) // Apply standard gravity if just in the air
        {
            // We can rely on standard Rigidbody gravity or add a custom gravity scale here if needed
            // rb.AddForce(Physics.gravity * rb.mass); // Standard gravity already applied by Unity Physics
        }
    }

    void LimitFallSpeed()
    {
        // Clamp vertical velocity to prevent excessive fall speeds
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
        }
    }


    #endregion

    #region Dashing Logic

    void HandleDashMovement()
    {
        // Dash logic is handled by setting velocity directly when starting the dash
        // Optionally, slightly modify gravity during dash if desired
        rb.useGravity = false; // Example: Turn off gravity during dash
    }

    void TryDash()
    {
        if (dashInputDown && canDash && dashCooldownTimer <= 0f && !isKnockedBack)
        {
            StartDash();
        }
    }

    void StartDash()
    {
        isDashing = true;
        canDash = false; // Use up the dash charge
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Determine dash direction
        Vector3 dashDirection = new Vector3(horizontalInput, verticalInput, 0f);
        if (dashDirection.sqrMagnitude < 0.01f) // If no input, dash forward relative to player facing or camera
        {
            // Simplest: Dash in the direction the player is visually facing (might need adjustment)
            dashDirection = transform.forward;
            // More common: Dash based on camera forward if no input
            // dashDirection = Camera.main.transform.forward;
            // dashDirection.y = 0; // Keep dash horizontal if defaulting
        }
        dashDirection = dashDirection.normalized;


        // Store current vertical velocity to potentially re-apply partially after dash
        float previousYVelocity = rb.velocity.y;

        // Override velocity for the dash
        rb.velocity = dashDirection * dashSpeed;

        // Option: Give a small vertical boost if dashing upwards diagonally
        if (Vector3.Dot(dashDirection, Vector3.up) > 0.1f) // Check if dashing somewhat upwards
        {
            // Add a bit of upward velocity, or clamp the Y component of the dash velocity
            // Example: Clamp max upward dash speed component
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Min(rb.velocity.y, dashSpeed * 0.5f), rb.velocity.z);
        }


        // Temporarily disable gravity (optional)
        // rb.useGravity = false;

        // Stop grapple if dashing
        if (isGrappling) StopGrapple();
        // Stop jetpack if dashing
        isJetpacking = false;
    }


    void StopDash()
    {
        isDashing = false;
        rb.useGravity = true; // Re-enable gravity if it was turned off

        // Reset velocity or apply a portion of the previous vertical velocity
        // This prevents instantly stopping or falling like a rock after a dash
        rb.velocity = new Vector3(rb.velocity.x * 0.5f, rb.velocity.y * dashEndYMultiplier, rb.velocity.z * 0.5f); // Keep some momentum

        // Allow dash reset on ground immediately if applicable
        if (isGrounded && resetDashOnGround) canDash = true;
    }

    #endregion

    #region Grapple Hook Logic

    void TryGrapple()
    {
        if (grappleInputDown && !isDashing && !isKnockedBack)
        {
            StartGrapple();
        }
        if (grappleInputUp && isGrappling)
        {
            StopGrapple();
        }
    }

    void StartGrapple()
    {
        // Raycast to find a grapple point
        RaycastHit hit;
        Vector3 rayOrigin = transform.position; // Or Camera.main.transform.position
        Vector3 rayDirection = Camera.main.transform.forward; // Or mouse position converted to world direction

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, grappleLayerMask))
        {
            if (hit.distance > grappleRange) return;
            Debug.Log("Start Grapple");
            grapplePoint = hit.point;
            isGrappling = true;

            if (usePhysicsJointForGrapple)
            {
                // --- Physics Joint Method ---
                if (!grappleJoint) grappleJoint = gameObject.AddComponent<SpringJoint>();
                grappleJoint.autoConfigureConnectedAnchor = false;
                grappleJoint.connectedAnchor = grapplePoint;
                grappleJoint.spring = grappleJointSpring;
                grappleJoint.damper = grappleJointDamper;
                grappleJoint.minDistance = grappleStopDistance;
                grappleJoint.maxDistance = Vector3.Distance(transform.position, grapplePoint) * 0.8f; // Initial length
            }
            else
            {
                // --- Manual Force Method ---
                rb.velocity = Vector3.zero; // Optional: Stop current momentum
                                            // Movement handled in FixedUpdate
            }


            // Optional: Enable and position Line Renderer
            if (grappleLineRenderer)
            {
                grappleLineRenderer.enabled = true;
                grappleLineRenderer.SetPosition(0, transform.position); // Start point (player)
                grappleLineRenderer.SetPosition(1, grapplePoint);      // End point (hit)
            }

            // Stop jetpack if grappling
            isJetpacking = false;
        }
        else
        {
            Debug.Log("Fail to grapple");
        }
    }

    void HandleGrappleMovement()
    {
        if (!usePhysicsJointForGrapple)
        {
            // --- Manual Force Method ---
            float distanceToPoint = Vector3.Distance(transform.position, grapplePoint);

            if (distanceToPoint > grappleStopDistance)
            {
                Vector3 directionToPoint = (grapplePoint - transform.position).normalized;
                rb.AddForce(directionToPoint * grappleSpeed, ForceMode.Acceleration);

                // Optional: Limit speed towards grapple point
                // float speedTowardsGrapple = Vector3.Dot(rb.velocity, directionToPoint);
                // if (speedTowardsGrapple > grappleSpeed) {
                //     rb.velocity = rb.velocity - directionToPoint * (speedTowardsGrapple - grappleSpeed);
                // }
            }
            else
            {
                // Close enough, stop pulling force or transition to swing
                rb.velocity *= 0.9f; // Dampen velocity when close
                                     // Optionally could stop grapple here automatically: StopGrapple();
            }
        }
        // Else: Physics Joint handles the movement implicitly
    }


    void StopGrapple()
    {
        isGrappling = false;

        if (usePhysicsJointForGrapple && grappleJoint)
        {
            Destroy(grappleJoint);
        }

        // Optional: Disable Line Renderer
        if (grappleLineRenderer)
        {
            grappleLineRenderer.enabled = false;
        }
    }

    void UpdateGrappleLine()
    {
        if (isGrappling && grappleLineRenderer && grappleLineRenderer.enabled)
        {
            grappleLineRenderer.SetPosition(0, transform.position); // Update player end dynamically
        }
    }


    #endregion

    #region Jetpack Logic

    void HandleJetpackFuelRegen()
    {
        // If not using jetpack and regen delay is over
        if (!isJetpacking && currentFuel < maxFuel && fuelRegenTimer <= 0f)
        {
            currentFuel += fuelRegenRate * Time.deltaTime;
            currentFuel = Mathf.Min(currentFuel, maxFuel); // Clamp to max
        }
    }

    void HandleJetpackMovement()
    {
        // Check input and not grappling/dashing, and has fuel
        if (jetpackInputHeld && currentFuel > 0f && !isGrappling && !isDashing)
        {
            isJetpacking = true;
            rb.AddForce(Vector3.up * jetpackForce, ForceMode.Acceleration);
            currentFuel -= fuelConsumeRate * Time.fixedDeltaTime;
            currentFuel = Mathf.Max(currentFuel, 0f); // Clamp to min
            fuelRegenTimer = fuelRegenDelay; // Reset regen delay timer
        }
        else
        {
            // If button released or out of fuel or grounded/grappling/dashing
            if (isJetpacking)
            {
                isJetpacking = false;
                // No need to reset regen timer here, HandleTimers does that
            }
        }
    }


    // Optional: Public getter for fuel percentage (for UI)
    public float GetFuelPercentage()
    {
        return currentFuel / maxFuel;
    }

    #endregion

    #region Knockback Logic

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        // Prevent knockback during dash or grapple maybe? (Optional)
        if (isDashing || isGrappling) return;

        StopDash(); // Cancel dash if hit
        StopGrapple(); // Cancel grapple if hit
        isJetpacking = false; // Stop jetpack if hit

        isKnockedBack = true;
        knockbackTimer = duration;

        // Apply resistance
        float effectiveForce = force * (1f - Mathf.Clamp01(knockbackResistance));

        // Apply knockback force
        rb.velocity = Vector3.zero; // Reset velocity before applying impulse
        rb.AddForce(direction.normalized * effectiveForce, ForceMode.Impulse);

        Debug.Log($"Applied Knockback: Dir={direction.normalized}, Force={effectiveForce}, Duration={duration}");
    }

    void StopKnockback()
    {
        isKnockedBack = false;
        // Optionally reset velocity or let physics take over
        // rb.velocity *= 0.5f; // Dampen velocity after knockback ends
    }

    #endregion

    #region Gizmos (for debugging in Scene view)

    void OnDrawGizmos()
    {
        // Draw Ground Check sphere
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }

        // Draw Grapple Raycast direction (if needed)
        // Gizmos.color = Color.blue;
        // Gizmos.DrawLine(transform.position, transform.position + Camera.main.transform.forward * grappleRange);
    }

    #endregion
}