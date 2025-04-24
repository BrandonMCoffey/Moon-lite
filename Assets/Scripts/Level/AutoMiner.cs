using UnityEngine;
using Sirenix.OdinInspector;

public class AutoMiner : PlaceableBase
{
    [SerializeField] private int _width = 2;
    [SerializeField] private float _destroyTime = 0.5f;
    [SerializeField] private float _groundOffset = 0.25f;
    [SerializeField] private LayerMask _groundLayer = 1;

    [Header("Debug")]
    [SerializeField, ReadOnly] private float _destroyTimer;
    [SerializeField, ReadOnly] private int _currentTileX;
    [SerializeField, ReadOnly] private float _velocity = 0f;
    [SerializeField, ReadOnly] private bool _grounded = false;

    private void Update()
    {
        _destroyTimer += Time.deltaTime;
        if (_destroyTimer > _destroyTime)
        {
            _destroyTimer -= _destroyTime;
            DestroyNextTile();
        }
    }

    private void FixedUpdate()
    {
        ApplyGravity(Time.fixedDeltaTime);
    }

    private void DestroyNextTile()
    {
        if (!_grounded) return;

        for (int i = 0; i < _width; i++)
        {
            var pos = transform.position + Vector3.down * _world.TileSizeMult + Vector3.right * (_currentTileX * _world.TileSizeMult);
            if (_world.DamageTileAt(pos, 1))
            {
                return;
            }
            _currentTileX++;
            if (_currentTileX >= _width) _currentTileX = 0;
        }
    }

    private void ApplyGravity(float deltaTime)
    {
        float offset = _velocity * deltaTime;
        RaycastHit hit;
        _grounded = Physics.Raycast(transform.position, -transform.up, out hit, offset + _groundOffset, _groundLayer);
        bool g2 = Physics.Raycast(transform.position + transform.right * _world.TileSizeMult, -transform.up, out var hit2, offset + _groundOffset, _groundLayer);
        if (g2)
        {
            if (!_grounded || hit2.distance < hit.distance) hit = hit2;
            _grounded = true;
        }

        if (_grounded)
        {
            _velocity = 0f;
            if (hit.distance > _groundOffset)
            {
                transform.position += new Vector3(0f, hit.distance - _groundOffset, 0f);
            }
        }
        else
        {
            _velocity += -9.81f * deltaTime;
            _velocity = Mathf.Max(_velocity, -20f);
            transform.position += new Vector3(0, offset, 0);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < _width; i++)
        {
            Gizmos.DrawRay(transform.position - transform.up * _groundOffset + transform.right * i, -transform.up);
        }
    }
#endif
}
