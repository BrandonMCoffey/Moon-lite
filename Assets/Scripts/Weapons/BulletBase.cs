using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class BulletBase : SimplePhysicsObject
{
    [SerializeField] protected MineralData _data;

    [Header("Bullet Data (Dynamic from weapon and bullet data)")]
    [SerializeField, ReadOnly] protected float _shotSpeed = 5;
    [SerializeField, ReadOnly] protected float _damage = 1;
    [SerializeField, ReadOnly] protected float _knockback = 3;
    [SerializeField, ReadOnly] protected float _size = 1;
    [SerializeField, ReadOnly] protected float _lifetime = 3;
    [SerializeField, ReadOnly] protected int _bounces = 0;
    [SerializeField, ReadOnly] protected bool _hitOnBounce = true;
    [SerializeField, ReadOnly] private bool _hasAOE = false;
    [SerializeField, ReadOnly] private float _aoeRadius = 1f;

    public LayerMask BulletLayerMask => _physicsLayerMask;

    protected SpriteRenderer _sr;
    //protected TrailRenderer _tr;
    protected Vector3 _startPosition;
    protected bool _isVisible = true;

    protected int _pierceAmount = 0;

    protected override void Awake()
    {
        base.Awake();
        //_rb = GetComponent<Rigidbody>();
        _sr = GetComponent<SpriteRenderer>();
        //_tr = GetComponent<TrailRenderer>();
        ResetBullet();
    }

    protected override void Update()
    {
        base.Update();

        // Rotate the bullet to face the direction of movement
        float angle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        //transform.rotation = Quaternion.LookRotation(_velocity);

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

    public void ResetBullet()
    {
        _sr.enabled = true; // Initially hide the bullet
        //_tr.enabled = false; // Initially hide the trail
        _startPosition = transform.position;
        ResetPhysics();
    }

    public virtual void BulletSetup(Vector3 shootDir, float angle, WeaponData weaponData, LayerMask hitLayer, MineralData overrideData = null)
    {
        if (overrideData != null) _data = overrideData;
        _shotSpeed = _data.ShotSpeed * weaponData.BulletSpeed;
        _damage = _data.Damage * weaponData.BulletDamage;
        _knockback = _data.Knockback * weaponData.BulletKnockback;
        _size = _data.Size * weaponData.BulletSize;
        _lifetime = _data.Lifetime * weaponData.BulletLifetime;
        _gravityScale = _data.GravityScale * weaponData.BulletGravityScale;
        _bounces = _data.Bounces + weaponData.BulletBounces;
        _hitOnBounce = _data.HitOnBounce || weaponData.ForceHitOnBounce;
        _hasAOE = _data.HasAOE || weaponData.ForceAoe;
        _aoeRadius = _data.AoeRadius * weaponData.BulletAoeRadius;

        _physicsLayerMask = BulletLayerMask;

        _pierceAmount = Mathf.CeilToInt(_damage * 7.5f);

        _sr.sprite = _data.BulletSprite;

        // Too small to use AOE (Might happen because bullet is too weak)
        if (_aoeRadius < 0.5f) _hasAOE = false;

        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        SetVelocity(shootDir * _shotSpeed);
        //_velocity = shootDir * _shotSpeed;

        switch(_data.TileType)
        {
        // if the weapon already has AOE, make the mult a bit smaller
        case TileType.Explosive:
            if(weaponData.ForceAoe)
            {
                _aoeRadius = weaponData.BulletAoeRadius * _data.AoeRadius * 0.5f;
            }
            else
            {
                _aoeRadius = weaponData.BulletAoeRadius * _data.AoeRadius;
            }
            break;
        }

        StartCoroutine(DelayedDestroy(_lifetime));

        PreRaycastCheck();
    }

    protected virtual IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        DestroySelf();
    }

    protected virtual void DestroySelf()
    {
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
        //Destroy(gameObject);
        BulletPooling.AddBulletToPool(this);
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
            if (_bounces > 0)
            {
                _bounces--;
                if(_hitOnBounce)
                {
                    base.OnHit(hit);
                }
                return;
            }
        }

        // Note: This uses bulletLayer (_physicsLayerMask) for what can be hit
        var damageable = hit.collider.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(_damage);
            DestroySelf();
        }

        // Bullet impact feedback
        Vector3 hitParticleDir = Vector3.Lerp(hit.normal, -transform.right, 0.6f);
        hitParticleDir.z = 0;
        BulletPooling.BulletHitFeedback(_data.Color, hit.point, hitParticleDir.normalized);
        _data.PlayHitSfx();

        // TODO: Fix bullets impacting at the same time as each other (Same with raycasts)

        if (_data.TileType == TileType.Aethergem)
        {
            if (_pierceAmount > 0)
            {
                _pierceAmount -= 1;
                return; // Dont destroy
            }
        }
        DestroySelf();
    }

    // Override this to customize chunk hit behavior
    public virtual void OnHitChunk(Chunk chunk, Vector3 hitPoint)
    {
        if (_hasAOE)
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

public class BulletSpawner
{
    public static Vector3 ShootBullet(WeaponData weaponData, MineralData bulletData, LayerMask bulletLayer, Vector3 spawnPosition, Vector3 aimDirection)
    {
        aimDirection.z = 0; // Ensure z-axis is zero for 2D shooting
        aimDirection = aimDirection.normalized;
        Vector3 knockback = -aimDirection * weaponData.BulletKickback * bulletData.Kickback;

        // Apply spray effect
        float randomAngle = Random.Range(-weaponData.SprayAngle * 0.5f, weaponData.SprayAngle * 0.5f);
        Vector3 preSpreadAim = aimDirection;
        aimDirection = Quaternion.Euler(0, 0, randomAngle) * aimDirection;

        if (weaponData.HitScan)
        {
            // Adjust the raycast start position and range
            Vector3 adjustedStart = spawnPosition - aimDirection.normalized * .75f; // Start slightly behind
            float adjustedRange = weaponData.RayRange + .75f; // Extend the range to compensate

            for (int i = 0; i < (weaponData.MultipleBullets ? weaponData.BulletAmount : 1); i++)
            {
                // Apply spread for each raycast
                float spreadAngle = Random.Range(-weaponData.SprayAngle * 0.5f, weaponData.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                int remainingBounces = weaponData.BulletBounces; // Number of allowed bounces
                Vector3 currentStart = adjustedStart;
                Vector3 currentDirection = spreadDirection;

                while (remainingBounces >= 0)
                {
                    if (Physics.Raycast(currentStart, currentDirection, out RaycastHit hit, adjustedRange, bulletData.BulletPrefab.BulletLayerMask, QueryTriggerInteraction.Ignore))
                    {
                        var chunk = hit.collider.GetComponent<Chunk>();
                        if (chunk != null)
                        {
                            Vector3 pointToDestroy = hit.point; // Use the actual hit point

                            // Apply ammo type effects
                            if (weaponData.ForceAoe || bulletData.HasAOE)
                            {
                                // if the weapon already has AOE, make the mult a bit smaller
                                float aoeRadius = weaponData.BulletAoeRadius;

                                switch (bulletData.TileType)
                                {
                                    case TileType.Explosive:
                                        if (weaponData.ForceAoe)
                                        {
                                            aoeRadius = weaponData.BulletAoeRadius * bulletData.AoeRadius * 0.5f;
                                        }
                                        else
                                        {
                                            aoeRadius = weaponData.BulletAoeRadius * bulletData.AoeRadius;
                                        }
                                        break;
                                }

                                chunk.World.DamageTileInCircle(pointToDestroy, aoeRadius, weaponData.BulletDamage * bulletData.Damage);
                            }
                            else
                            {
                                chunk.World.DamageTileAt(pointToDestroy, weaponData.BulletDamage * bulletData.Damage);
                            }
                        }

                        // Visualize the ray with a LineRenderer
                        BulletPooling.CreateLaserVisual(currentStart, hit.point, weaponData.RayColor, weaponData.BulletSize);

                        // Reflect the raycast direction
                        currentDirection = Vector3.Reflect(currentDirection, hit.normal);
                        currentStart = hit.point + currentDirection * 0.01f; // Offset to avoid immediate re-hit
                        remainingBounces--;
                        // TODO: Put this in SO data object
                        adjustedRange *= 0.667f;
                    }
                    else
                    {
                        // Visualize the final ray segment if no hit occurs
                        BulletPooling.CreateLaserVisual(currentStart, currentStart + currentDirection * adjustedRange, weaponData.RayColor, weaponData.BulletSize);
                        break; // Exit if no hit
                    }
                }
            }
        }
        else if (weaponData.MultipleBullets)
        {
            // Handle multiple bullets logic
            for (int i = 0; i < weaponData.BulletAmount; i++)
            {
                float spreadAngle = Random.Range(-weaponData.SprayAngle * 0.5f, weaponData.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                BulletBase bulletInstance = BulletPooling.SpawnBullet(spawnPosition);
                float angle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;

                bulletInstance.BulletSetup(spreadDirection, angle, weaponData, bulletLayer, bulletData);
            }
        }
        else
        {
            // Default single bullet logic
            BulletBase bulletInstance = BulletPooling.SpawnBullet(spawnPosition);
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            bulletInstance.BulletSetup(aimDirection, angle, weaponData, bulletLayer, bulletData);
        }

        return knockback;
    }
}