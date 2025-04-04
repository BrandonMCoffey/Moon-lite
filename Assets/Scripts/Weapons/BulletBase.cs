using System.Collections;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] private float _shotSpeed = 5;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _knockback = 3;
    [SerializeField] private float _size = 1;
    [SerializeField] private float _lifetime = 3;
    [SerializeField] private float _gravityScale = 0f; // Artificial gravity scale

    //[SerializeField] private GameObject _psBloodDirectional;
    //[SerializeField] private GameObject _psBloodCircle;
    //[SerializeField] private GameObject _psOtherDirectional;

    private Rigidbody _rb;
    private bool _once = false;
    private SpriteRenderer _sr;
    //private TrailRenderer _tr;
    private Vector3 _startPosition;
    private bool _isVisible = false;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _sr = GetComponent<SpriteRenderer>();
        //_tr = GetComponent<TrailRenderer>();
        _sr.enabled = false; // Initially hide the bullet
        //_tr.enabled = false; // Initially hide the trail

        _startPosition = transform.position;
    }

    private void Update()
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

    private void OnTriggerEnter(Collider collision)
    {
        var chunk = collision.GetComponent<Chunk>();
        if (chunk != null)
        {
            Vector3 pointToDestroy = transform.position + (transform.forward * 0.1f);
            chunk.World.DestroyTileAt(pointToDestroy, 2);
            Destroy();
        }
        // if (collision.CompareTag("BulletBounds") && !_once)
        // {
        //     Destroy();
        // }
        // else if (collision.GetComponent<EnemyHealth>() != null)
        // {
        //     HandleEnemyCollision(collision);
        // }
        // else
        // {
        //     Debug.Log($"Hit {collision.name} with bullet {gameObject.name}");
        // }
    }

    public void BulletSetup(Vector3 shootDir, float angle, float shotSpeed, int damage, float knockback, float size, float lifetime, float gravityScale)
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

    //     Destroy();
    // }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy();
    }

    private void Destroy()
    {
        if (!_once)
        {
            _once = true;
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}