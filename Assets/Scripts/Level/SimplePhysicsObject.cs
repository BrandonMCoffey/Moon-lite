using UnityEngine;
using Sirenix.OdinInspector;

public class SimplePhysicsObject : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] protected bool _physicsEnabled = true;
    [SerializeField] protected float _gravityScale = 2f;
    [SerializeField] private float _maxVelocity = 20;
    [SerializeField] protected LayerMask _physicsLayerMask = 1;
    [SerializeField] private float _radius = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _bounceVelocityMultiplierX = 0.8f;
    [SerializeField, Range(0f, 1f)] private float _bounceVelocityMultiplierY = 0.4f;
    [SerializeField, Range(0f, 1f)] private float _drag = 0.5f;

    [Header("Physics Debug")]
    [SerializeField, ReadOnly] protected Vector3 _velocity;
    [SerializeField, ReadOnly] protected bool _grounded;
    [SerializeField, ReadOnly] private float _groundedTimer;
    private Vector3 previousPosition;
    private Vector3 currentPosition;

    protected virtual void OnEnable()
    {
        ResetPhysics();
    }

    protected virtual void Update()
    {
        if (!_physicsEnabled) return;

        InterpolatePosition();
    }

    private void FixedUpdate()
    {
        if (!_physicsEnabled) return;

        if (_grounded && _groundedTimer >= 0f)
        {
            _groundedTimer -= Time.fixedDeltaTime;
            return;
        }
        previousPosition = currentPosition;
        ApplyGravity(Time.fixedDeltaTime);
        ApplyVelocity(Time.fixedDeltaTime);
        currentPosition = transform.position;
    }

    private void InterpolatePosition()
    {
        float interpolationFactor = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        Vector3 interpolatedPosition = Vector3.Lerp(previousPosition, currentPosition, interpolationFactor);
        transform.position = interpolatedPosition;
    }

    public void SetPhysicsEnabled(bool enabled)
    {
        if (_physicsEnabled == enabled) return;
        _physicsEnabled = enabled;
        ResetPhysics();
    }

    protected void ResetPhysics()
    {
        _velocity = Vector3.zero;
        previousPosition = transform.position;
        currentPosition = transform.position;
    }

    protected virtual void ApplyGravity(float deltaTime)
    {
        _velocity += Vector3.up * (-9.81f * _gravityScale * deltaTime);
        _velocity *= 1f - _drag * deltaTime;
    }

    protected virtual void ApplyVelocity(float deltaTime)
    {
        Vector3 offset = _velocity * deltaTime;
        if (Physics.Raycast(transform.position, _velocity.normalized, out var hit, offset.magnitude + _radius, _physicsLayerMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
            // Match object to ground
            if (hit.distance > _radius)
            {
                transform.position += offset.normalized * (hit.distance - _radius);
            }
            if (_velocity.magnitude < 0.25f)
            {
                _grounded = true;
                // Reduce physics calls when grounded
                _groundedTimer = 0.1f;
            }
        }
        else
        {
            _grounded = false;
            transform.position += offset;
        }
    }

    protected virtual void OnHit(RaycastHit hit)
    {
        var vel = Vector3.Reflect(_velocity, hit.normal);
        vel.x *= _bounceVelocityMultiplierX;
        vel.y *= _bounceVelocityMultiplierY;
        SetVelocity(vel);
    }

    public void SetVelocity(Vector3 velocity)
    {
        _velocity = velocity;
        if (_velocity.magnitude > _maxVelocity) _velocity = _velocity.normalized * _maxVelocity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif
}
