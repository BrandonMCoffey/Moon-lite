using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Moonlite/MovementData")]
public class CustomMovementData : ScriptableObject
{
    [Header("Gravity")]
    [Tooltip("Downwards force (gravity) needed for the desired jumpHeight and jumpTimeToApex")]
    [ReadOnly] public float gravityStrength;
    [Tooltip("Strength of the player's gravity as a multiplier of gravity (set in ProjectSettings/Physics2D)")]
    [ReadOnly] public float gravityScale;
    [Tooltip("Multiplier to the player's gravityScale when falling")]
    public float fallGravityMult = 1.5f;
    [Tooltip("Maximum fall speed (terminal velocity) of the player when falling")]
    public float maxFallSpeed = 25f;
    [Tooltip("Larger multiplier to the player's gravityScale when they are falling and a downwards input is pressed")]
    public float fastFallGravityMult = 2f;
    [Tooltip("Maximum fall speed(terminal velocity) of the player when performing a faster fall")]
    public float maxFastFallSpeed = 30f;

    [Header("Run")]
    [Tooltip("Target speed we want the player to reach")]
    public float runMaxSpeed = 11f;
    [Tooltip("The speed at which our player accelerates to max speed, can be set to runMaxSpeed for instant acceleration down to 0 for none at all")]
    public float runAcceleration = 2.5f;
    [ReadOnly]
    [Tooltip("The actual force (multiplied with speedDiff) applied to the player")]
    public float runAccelAmount;
    [Tooltip("The speed at which our player decelerates from their current speed, can be set to runMaxSpeed for instant deceleration down to 0 for none at all")]
    public float runDecceleration = 5f;
    [ReadOnly]
    [Tooltip("Actual force (multiplied with speedDiff) applied to the player")]
    public float runDeccelAmount;
    [Range(0f, 1)]
    public float accelInAir = 0.65f;
    [Range(0f, 1)]
    public float deccelInAir = 0.65f;
    public bool doConserveMomentum = true;

    [Header("Jump")]
    public float jumpHeight = 3.5f;
    [Tooltip("Time between applying the jump force and reaching the desired jump height. These values also control the player's gravity and jump force")]
    public float jumpTimeToApex = 0.3f;
    [ReadOnly]
    [Tooltip("The actual force applied (upwards) to the player when they jump")]
    public float jumpForce;
    [Range(0f, 1)]
    [Tooltip("Reduces gravity while close to the apex (desired max height) of the jump")]
    public float jumpHangGravityMult = 0.5f;
    [Tooltip("Speeds (close to 0) where the player will experience extra \"jump hang\". The player's velocity.y is closest to 0 at the jump's apex (think of the gradient of a parabola or quadratic function)")]
    public float jumpHangTimeThreshold = 1f;
    public float jumpHangAccelerationMult = 1.1f;
    public float jumpHangMaxSpeedMult = 1.3f;

    [Header("Assists")]
    [Range(0.01f, 0.5f)]
    [Tooltip("Grace period after falling off a platform, where you can still jump")]
    public float coyoteTime = 0.1f;
    [Range(0.01f, 0.5f)]
    [Tooltip("Grace period after pressing jump where a jump will be automatically performed once the requirements (eg. being grounded) are met")]
    public float jumpInputBufferTime = 0.1f;

    [Header("Dash")]
    public int dashAmount = 1;
    public float dashSpeed = 20f;
    [Tooltip("Duration for which the game freezes when we press dash but before we read directional input and apply a force")]
    public float dashSleepTime = 0.05f;
    public float dashAttackTime = 0.15f;
    [Tooltip("Time after you finish the initial drag phase, smoothing the transition back to idle (or any standard state)")]
    public float dashEndTime = 0.15f;
    [Tooltip("Slows down player, makes dash feel more responsive (used in Celeste)")]
    public Vector2 dashEndSpeed = new Vector2(15, 15);
    [Range(0f, 1f)]
    [Tooltip("Slows the affect of player movement while dashing")]
    public float dashEndRunLerp = 0.5f;
    public float dashRefillTime = 0.1f;
    [Range(0.01f, 0.5f)]
    public float dashInputBufferTime = 0.1f;
    public float dashJumpBonusBufferTime = 0.1f;

    [Header("Jetpack")]
    public float jetpackForce = 25f;
    public float jetpackConsumeRate = 0.5f;
    public float jetpackRegenDelay = 0.5f;
    public float jetpackRegenRate = 0.3f;

    private void OnValidate()
    {
        //Calculate gravity strength using the formula (gravity = 2 * jumpHeight / timeToJumpApex^2) 
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        //Calculate the rigidbody's gravity scale (ie: gravity strength relative to unity's gravity value, see project settings/Physics)
        gravityScale = gravityStrength / Physics.gravity.y;

        //Calculate are run acceleration & deceleration forces using formula: amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        //Calculate jumpForce using the formula (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
    }
}