using System.Collections;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] protected float _shotSpeed = 5;
    [SerializeField] protected int _damage = 1;
    [SerializeField] protected float _knockback = 3;
    [SerializeField] protected float _size = 1;
    [SerializeField] protected float _lifetime = 3;
    [SerializeField] protected float _gravityScale = 0f; // Artificial gravity scale

    //[SerializeField] private GameObject _psBloodDirectional;
    //[SerializeField] private GameObject _psBloodCircle;
    //[SerializeField] private GameObject _psOtherDirectional;

    protected Rigidbody _rb;
    protected SpriteRenderer _sr;
    //protected TrailRenderer _tr;
    protected Vector3 _startPosition;
    protected bool _once = false;
    protected bool _isVisible = false;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _sr = GetComponent<SpriteRenderer>();
        //_tr = GetComponent<TrailRenderer>();
        _sr.enabled = false; // Initially hide the bullet
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
        // Apply artificial gravity to the y-axis
        if (_gravityScale != 0f)
        {
            float gravity = _gravityScale * -9.81f * Time.deltaTime;
            _rb.velocity += new Vector3(0, gravity, 0); // Add gravity to the velocity
        }

        if (!_isVisible && Vector3.Distance(_startPosition, transform.position) >= 0.5f)
        {
            _sr.enabled = true; // Show the bullet after it has moved x units
            //_tr.enabled = true; // Show the trail after it has moved x units
            _isVisible = true;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        var chunk = collision.gameObject.GetComponent<Chunk>();
        if (chunk != null)
        {
            Vector3 pointToDestroy = collision.GetContact(0).point;
            OnHitChunk(chunk, pointToDestroy);
            DestroySelf();
        }
    }

    // Override this to customize chunk hit behavior
    protected virtual void OnHitChunk(Chunk chunk, Vector3 hitPoint)
    {
        chunk.World.DamageTileAt(hitPoint);
        //chunk.World.DamageTileInCircle(hitPoint,2);
    }

    public virtual void BulletSetup(Vector3 shootDir, float angle, float shotSpeed, int damage, float knockback, float size, float lifetime, float gravityScale)
    {
        _shotSpeed = shotSpeed;
        _damage = damage;
        _knockback = knockback;
        _size = size;
        _lifetime = lifetime;
        _gravityScale = gravityScale;

        _rb = GetComponent<Rigidbody>();
        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        _rb.AddForce(shootDir * _shotSpeed, ForceMode.Impulse);

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
}
