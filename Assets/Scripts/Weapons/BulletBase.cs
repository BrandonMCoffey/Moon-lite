using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class BulletBase : MonoBehaviour
{
    [SerializeField] protected MineralData _data;
    [SerializeField] private LayerMask _bulletCollisionMask = 1;

    [Header("Bullet Data (From weapon and bullet data)")]
    [SerializeField, ReadOnly] protected float _shotSpeed = 5;
    [SerializeField, ReadOnly] protected float _damage = 1;
    [SerializeField, ReadOnly] protected float _knockback = 3;
    [SerializeField, ReadOnly] protected float _size = 1;
    [SerializeField, ReadOnly] protected float _lifetime = 3;
    [SerializeField, ReadOnly] protected float _gravityScale = 0f; // Artificial gravity scale
    [SerializeField, ReadOnly] private bool _hasAOE = false;
    [SerializeField, ReadOnly] private float _aoeRadius = 1f;

    //[SerializeField] private GameObject _psBloodDirectional;
    //[SerializeField] private GameObject _psBloodCircle;
    //[SerializeField] private GameObject _psOtherDirectional;
    protected SpriteRenderer _sr;
    //protected TrailRenderer _tr;
    protected Vector3 _startPosition;
    protected bool _once = false;
    protected bool _isVisible = true;

    protected Vector3 _velocity;

    protected virtual void Awake()
    {
        //_rb = GetComponent<Rigidbody>();
        _sr = GetComponent<SpriteRenderer>();
        //_tr = GetComponent<TrailRenderer>();
        _sr.enabled = true; // Initially hide the bullet
        //_tr.enabled = false; // Initially hide the trail

        _startPosition = transform.position;
    }

    // Checks if the bullet spawns inside a wall/collision
    // NEEDS TO CHECK IF A TILE WAS ACTUALLY DAMAGED TO DESTROY THE BULLET
    // void Start()
    // {
    //     Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
    //     foreach (var hitCollider in hitColliders)
    //     {
    //         var chunk = hitCollider.GetComponent<Chunk>();
    //         if (chunk != null)
    //         {
    //             chunk.World.DamageTileAt(transform.position);
    //             //chunk.World.DamageTileInCircle(transform.position, 2);
    //             DestroySelf();
    //             break; // Exit the loop after handling the first collision
    //         }
    //     }
    // }

    protected virtual void Update()
    {
        ApplyGravity();
        RaycastCheck();

        // if (!_isVisible && Vector3.Distance(_startPosition, transform.position) >= 0.5f)
        // {
        //     _sr.enabled = true; // Show the bullet after it has moved x units
        //     //_tr.enabled = true; // Show the trail after it has moved x units
        //     _isVisible = true;
        // }
    }

    // protected virtual void OnCollisionEnter(Collision collision)
    // {
    //     var chunk = collision.gameObject.GetComponent<Chunk>();
    //     if (chunk != null)
    //     {
    //         Vector3 pointToDestroy = collision.GetContact(0).point + _rb.velocity.normalized * 0.1f;
    //         OnHitChunk(chunk, pointToDestroy);
    //     }

    //     DestroySelf();
    // }

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

    public virtual void BulletSetup(Vector3 shootDir, float angle, WeaponData weaponData)
    {
        _shotSpeed = _data.ShotSpeed * weaponData.BulletSpeed;
        _damage = _data.Damage * weaponData.BulletDamage;
        _knockback = _data.Knockback * weaponData.BulletKnockback;
        _size = _data.Size * weaponData.BulletSize;
        _lifetime = _data.Lifetime * weaponData.BulletLifetime;
        _gravityScale = _data.GravityScale * weaponData.BulletGravityScale;
        _hasAOE = _data.HasAOE;
        _aoeRadius = _data.AoeRadius * weaponData.BulletAoeRadius;

        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        _velocity = shootDir * _shotSpeed;

        StartCoroutine(DelayedDestroy(_lifetime));
    }

    // private void HandleEnemyCollision(Collider collision)
    // {
    //     collision.GetComponent<EnemyHealth>().TakeDamage(_rb.velocity.normalized * _knockback, _damage);

    //     Vector3 hitPoint = collision.ClosestPoint(transform.position);
    //     Vector3 bulletDirection = _rb.velocity.normalized;
    //     float angle = Mathf.Atan2(bulletDirection.y, bulletDirection.x) * Mathf.Rad2Deg;

    //     Instantiate(_psBloodDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
    //     Instantiate(_psBloodCircle, collision.transform.position, Quaternion.identity);

    //     if (Random.value < 0.2f)
    //     {
    //         Instantiate(_psOtherDirectional, hitPoint, Quaternion.Euler(0, 0, angle));
    //     }

    //     DestroySelf();
    // }

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

    protected void RaycastCheck()
    {
        if(Physics.Raycast(transform.position, _velocity.normalized, out RaycastHit hit, 0.5f, _bulletCollisionMask, QueryTriggerInteraction.Ignore))
        {
            var chunk = hit.collider.GetComponent<Chunk>();
            if (chunk != null)
            {
                Vector3 pointToDestroy = hit.point + _velocity.normalized * 0.01f;
                OnHitChunk(chunk, pointToDestroy);
            }

            DestroySelf();
        }
    }
    public void PreRaycastCheck()
    {
        //Debug.DrawRay(transform.position - _velocity.normalized * 2, _velocity.normalized * 2.5f, Color.red, 1.5f);

        if(Physics.Raycast(transform.position - _velocity.normalized * 2, _velocity.normalized, out RaycastHit hit, 2.5f, _bulletCollisionMask, QueryTriggerInteraction.Ignore))
        {
            //Debug.Log("Hit: " + hit.collider.name);
            var chunk = hit.collider.GetComponent<Chunk>();
            if (chunk != null)
            {
                Vector3 pointToDestroy = hit.point + _velocity.normalized * 0.01f;
                OnHitChunk(chunk, pointToDestroy);
            }

            DestroySelf();
        }
    }
}
