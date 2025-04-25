using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

public class BulletBase : SimplePhysicsObject
{
    [SerializeField] protected MineralData _data;

    [Header("Bullet Data (Dynamic from weapon and bullet data)")]
    [SerializeField, ReadOnly] protected float _shotSpeed = 5;
    [SerializeField, ReadOnly] protected float _damage = 1;
    [SerializeField, ReadOnly] protected float _knockback = 3;
    [SerializeField, ReadOnly] protected float _size = 1;
    [SerializeField, ReadOnly] protected float _lifetime = 3;
    [SerializeField, ReadOnly] private bool _hasAOE = false;
    [SerializeField, ReadOnly] private float _aoeRadius = 1f;

    public LayerMask BulletLayerMask => _physicsLayerMask;

    protected SpriteRenderer _sr;
    //protected TrailRenderer _tr;
    protected Vector3 _startPosition;
    protected bool _once = false;
    protected bool _isVisible = true;

    protected int _pierceAmount = 0;

    protected virtual void Awake()
    {
        //_rb = GetComponent<Rigidbody>();
        _sr = GetComponent<SpriteRenderer>();
        //_tr = GetComponent<TrailRenderer>();
        _sr.enabled = true; // Initially hide the bullet
        //_tr.enabled = false; // Initially hide the trail

        _startPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();

        // Rotate the bullet to face the direction of movement
        transform.rotation = Quaternion.LookRotation(Vector3.forward, _velocity.normalized);

        // NOTE: Merged with SimplePhysicsObject to only raycast on fixed update and use interpolation
        //ApplyGravity();
        //RaycastCheck();

        // if (!_isVisible && Vector3.Distance(_startPosition, transform.position) >= 0.5f)
        // {
        //     _sr.enabled = true; // Show the bullet after it has moved x units
        //     //_tr.enabled = true; // Show the trail after it has moved x units
        //     _isVisible = true;
        // }
    }

    public virtual void BulletSetup(Vector3 shootDir, float angle, WeaponData weaponData, MineralData overrideData = null)
    {
        if (overrideData != null) _data = overrideData;
        _shotSpeed = _data.ShotSpeed * weaponData.BulletSpeed;
        _damage = _data.Damage * weaponData.BulletDamage;
        _knockback = _data.Knockback * weaponData.BulletKnockback;
        _size = _data.Size * weaponData.BulletSize;
        _lifetime = _data.Lifetime * weaponData.BulletLifetime;
        _gravityScale = _data.GravityScale * weaponData.BulletGravityScale;
        _hasAOE = _data.HasAOE || weaponData.ForceAoe;
        _aoeRadius = _data.AoeRadius * weaponData.BulletAoeRadius;

        _pierceAmount = Mathf.CeilToInt(_damage * 7.5f);

        _sr.sprite = _data.BulletSprite;

        // Too small to use AOE (Might happen because bullet is too weak)
        if (_aoeRadius < 0.5f) _hasAOE = false;

        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        SetVelocity(shootDir * _shotSpeed);
        //_velocity = shootDir * _shotSpeed;

        PreRaycastCheck();

        StartCoroutine(DelayedDestroy(_lifetime));
    }

    protected virtual IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroySelf();
    }

    protected virtual void DestroySelf()
    {
        if (!_once)
        {
            _once = true;

            if (_hasAOE)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, _aoeRadius, _physicsLayerMask, QueryTriggerInteraction.Ignore);
                foreach (var collider in hitColliders)
                {
                    var chunk = collider.GetComponent<Chunk>();
                    if (chunk != null)
                    {
                        chunk.World.DamageTileInCircle(transform.position, _aoeRadius, _damage);
                    }
                }
            }

            StopAllCoroutines();
            Destroy(gameObject);
        }
    }

    protected void ApplyGravity()
    {
        // Apply artificial gravity to the y-axis
        if (_gravityScale != 0f)
        {
            float gravity = _gravityScale * -9.81f * Time.deltaTime;
            _velocity += new Vector3(0, gravity, 0); // Add gravity to the velocity
            transform.position += _velocity * Time.deltaTime;
        }
    }

    protected override void OnHit(RaycastHit hit)
    {
        var chunk = hit.collider.GetComponent<Chunk>();
        if (chunk != null)
        {
            Vector3 pointToDestroy = hit.point + _velocity.normalized * 0.01f;
            OnHitChunk(chunk, pointToDestroy);
        }

        // TODO: Enemy or other hit objects

        // TODO: Bullet impact feedback

        // TODO: Fix bullets impacting at the same time as each other (Same with raycasts)

        switch(_data.TileType)
        {
            case TileType.Aethergem:
                _pierceAmount -= 1;
                if (_pierceAmount <= 0)
                {
                    DestroySelf();
                }
                break;
            default:
                DestroySelf();
                break;
        }
    }

        // Override this to customize chunk hit behavior
    public virtual void OnHitChunk(Chunk chunk, Vector3 hitPoint)
    {
        if(_hasAOE)
        {
            chunk.World.DamageTileInCircle(hitPoint,_aoeRadius, _damage);
        }
        else
        {
            chunk.World.DamageTileAt(hitPoint, _damage);
        }
    }

    public void PreRaycastCheck()
    {
        if(Physics.Raycast(transform.position - _velocity.normalized * 2, _velocity.normalized, out RaycastHit hit, 2.5f, _physicsLayerMask, QueryTriggerInteraction.Ignore))
        {
            OnHit(hit);
        }
    }
}
