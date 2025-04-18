using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableBase : MonoBehaviour
{
    [SerializeField] private float _gravityScale = 2f;
    [SerializeField] private LayerMask _projectileLayerMask = 1;
    [SerializeField, Range(0f, 1f)] private float _velocityAfterBounce = 0.5f;
    [SerializeField] private float _drag = 0.5f;

    private Vector3 _velocity;

    private void Update()
    {
        _velocity += Vector3.up * (-9.81f * _gravityScale * Time.deltaTime);
        _velocity *= 1f - _drag * Time.deltaTime;

        Vector3 offset = _velocity * Time.deltaTime;
        if (Physics.Raycast(transform.position, _velocity.normalized, out var hit, offset.magnitude + 0.1f, _projectileLayerMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
        }
        transform.position += offset;
    }

    protected virtual void OnHit(RaycastHit hit)
    {
        _velocity = Vector3.Reflect(_velocity, hit.normal) * _velocityAfterBounce;
    }

    public virtual void Throw(Vector3 direction, float speed)
    {
        _velocity = direction * speed;
    }
}
