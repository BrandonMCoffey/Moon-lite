using UnityEngine;

//[CreateAssetMenu(fileName = "NewPCStats", menuName = "Player/Controller Stats")] // Added CreateAssetMenu for convenience
public class PCStats : ScriptableObject
{
    [Header("Layers")]
    [Tooltip("LayerMask for the player character itself. (Currently not used by PlayerControllerV2.cs)")]
    public LayerMask PlayerLayer; // Kept, but noted as unused by controller

    [Header("Input")]
    [Tooltip("Makes all Input snap to an integer. Prevents gamepads from walking slowly. Recommended value is true to ensure gamepad/keyboard similarity.")]
    public bool SnapInput = true;

    [Tooltip("Minimum input required before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
    public float VerticalDeadZoneThreshold = 0.3f; // Keep if used for ladders/ledges elsewhere

    [Tooltip("Minimum input required before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
    public float HorizontalDeadZoneThreshold = 0.1f;


    [Header("Movement")]
    [Tooltip("The top horizontal movement speed")]
    public float MaxSpeed = 14;

    [Tooltip("The player's capacity to gain horizontal speed ON GROUND")]
    public float Acceleration = 120;

    [Tooltip("The player's capacity to gain horizontal speed IN AIR")]
    public float AirAcceleration = 40; // <<< ADDED

    [Tooltip("The pace at which the player comes to a stop ON GROUND")]
    public float GroundDeceleration = 60;

    [Tooltip("The pace at which the player comes to a stop IN AIR (when input stops)")]
    public float AirDeceleration = 30;

    [Tooltip("A constant downward force applied while grounded. Helps keep player grounded on slopes/steps."), Range(0f, 10f)]
    public float StickToGroundForce = 2f; // <<< RENAMED (was GroundingForce), Range changed to positive

    [Tooltip("This is the base gravity applied to the player. It is a constant force that pulls the player down.")]
    public float Gravity = 50f;


    [Header("Jump")]
    [Tooltip("The immediate vertical velocity applied when jumping")]
    public float JumpPower = 36;

    [Tooltip("The maximum downward vertical speed (terminal velocity)")]
    public float MaxFallSpeed = 40;


    [Tooltip("The gravity multiplier applied when falling, or when jump is released early while still ascending.")]
    public float JumpEndEarlyGravityModifier = 3; // Tooltip slightly adjusted for clarity

    [Tooltip("The time before coyote jump becomes unusable after leaving a ledge.")]
    public float CoyoteTime = .15f;

    [Tooltip("The amount of time a jump input is buffered before hitting the ground.")]
    public float JumpBuffer = .2f;
}