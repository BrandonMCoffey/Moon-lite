using UnityEngine;
using System;
using System.Collections;

public abstract class WeaponBase : MonoBehaviour
{
    public event Action Fired = delegate { };

    [Header("Weapon Properties")]
    [SerializeField] protected string weaponName = "Default Weapon";
    public string WeaponName => weaponName;

    [SerializeField] protected Sprite _weaponSprite; // Sprite for the weapon
    public Sprite WeaponSprite => _weaponSprite;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    [SerializeField] protected Transform shootTransform;

    [Header("Reload and Ammo Properties")]
    [SerializeField] protected int _maxMagSize = 10; // Ammo capacity of the magazine.
    [SerializeField] protected float _reloadTime = 2f; // Time it takes to reload.
    public float ReloadTime => _reloadTime;
    protected int _currentMagAmmo; // Ammo left in the magazine.

    public int MaxMagSize => _maxMagSize;
    public int CurrentMagAmmo { get => _currentMagAmmo; set => _currentMagAmmo = value; }

    protected bool _isReloading = false;
    public bool IsReloading { get => _isReloading; set => _isReloading = value; }

    [Header("Fire Type")]
    [SerializeField] protected bool _isAutomatic = false;
    public bool IsAutomatic => _isAutomatic;

    [Tooltip("Bullets per second")]
    [SerializeField] protected float _fireRate = 4f; // Default to 4 bullets per second.
    protected float FireDelay => 1f / _fireRate; // Calculate delay dynamically.
    protected float _lastFireTime;

    [Header("Bullet Properties")]
    [SerializeField] protected GameObject _bulletPrefab;
    [SerializeField] protected GameObject _bulletParticlePrefab;
    [SerializeField] protected float _bulletSize = 1f;
    [SerializeField] protected float _bulletSpeed = 20f;
    [SerializeField] protected int _bulletDamage = 1;
    [SerializeField] protected float _bulletKnockback = 1f;
    [SerializeField] protected float _bulletLifetime = 3f;
    [SerializeField] protected float _bulletGravityScale = 0f;
    [SerializeField] protected bool _hasAOE = false;
    [SerializeField] protected float _aoeRadius = 1f;

    [Header("Ammo Consumption")]
    [SerializeField] protected int _ammoPerShot = 1; // Amount of ammo consumed per shot
    private int _tempAmmoPerShot = 0; // Amount of ammo consumed per shot
    public int AmmoPerShot => _ammoPerShot;

    [Header("Weapon Spray")]
    [SerializeField] protected float _sprayAngle = 0f; // Maximum spray angle in degrees

    protected Animator _anim;
    protected SpriteRenderer _sr;
    protected Rigidbody _rb;
    protected Collider _col;
    protected Transform _gunSprite;
    protected PlayerWeaponManager _pwm;
    private Coroutine _reloadCoroutine; // Track the reload coroutine

    private void Awake()
    {
        _currentMagAmmo = _maxMagSize;

        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _gunSprite = transform.Find("GunSprite");
        _pwm = FindObjectOfType<PlayerWeaponManager>();
        
        _weaponSprite = _sr.sprite;

        if (!_rb) Debug.LogWarning($"Rigidbody component is missing on {gameObject.name}");
        if (!_col) Debug.LogWarning($"Collider component is missing on {gameObject.name}");
        //if (!_anim) Debug.LogWarning($"Animator component is missing in children of {gameObject.name}");
        if (!_sr) Debug.LogWarning($"SpriteRenderer component is missing in children of {gameObject.name}");

        _tempAmmoPerShot = _ammoPerShot;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            if(_ammoPerShot !=0)
            {
                Debug.Log("Infinite ammo cheat activated for " + weaponName + "!");
                _ammoPerShot = 0;
            }
            else
            {
                Debug.Log("Infinite ammo cheat deactivated for " + weaponName + "!");
                _ammoPerShot = _tempAmmoPerShot;
            }
        }
    }

    public bool CanShoot()
    {
        return _currentMagAmmo > 0 && !_isReloading;
    }

    public virtual void Pickup(Transform newOwner)
    {
        Owner = newOwner;
        IsEquipped = true;

        // Parent the weapon to the new owner (weapon holder)
        transform.SetParent(newOwner);

        // Reset the weapon's local position and rotation relative to the weapon holder
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Disable physics and collision
        if (_rb != null) _rb.isKinematic = true;
        if (_col != null) _col.enabled = false;

        // Ensure the sprite child (GunSprite) has no local rotation
        if (_gunSprite != null)
        {
            _gunSprite.localRotation = Quaternion.identity;
            _gunSprite.GetComponent<SpriteRenderer>().sortingOrder = 5;
        }
    }

    public virtual void Drop()
    {
        IsEquipped = false;
        Owner = null;

        // Unparent the weapon from the weapon holder
        transform.SetParent(null);

        // Re-enable physics and collision
        if (_rb != null)
        {
            _rb.isKinematic = false; // Allow the weapon to interact with physics
        }
        if (_col != null)
        {
            _col.enabled = true; // Enable the collider
        }

        // Apply a force in the direction the sprite was rotated and add random rotational force
        if (_rb != null)
        {
            // Calculate the forward direction based on the weapon's current rotation
            Vector3 forwardDirection = transform.right;

            // Apply a force in the forward direction
            float dropForce = 5f; // Adjust the magnitude of the force as needed
            _rb.AddForce(forwardDirection * dropForce, ForceMode.Impulse);
        }

        // Ensure the sprite child (GunSprite) rotates naturally with the weapon
        if (_gunSprite != null)
        {
            _gunSprite.localRotation = Quaternion.identity; // Keep the sprite's local rotation consistent
        }

        // Ensure the weapon is visible and active
        gameObject.SetActive(true);
    }

    public virtual void Aim(Vector3 targetPosition)
    {
        // Replace the old aiming code with a raycast (For perspective camera)
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z));
        xy.Raycast(ray, out float distance);
        var pos = ray.GetPoint(distance);
        Vector3 aimDirection = (pos - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Rotate the weapon holder to aim at the target
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip the sprite vertically if aiming backward
        if (_gunSprite != null)
        {
            Vector3 localScale = _gunSprite.localScale;
            localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
            _gunSprite.localScale = localScale;
        }
    }

    // Overload: original Shoot signature for backwards compatibility
    public virtual void Shoot(Vector3 aimDirection)
    {
        Shoot(aimDirection, null);
    }

    // Now takes an optional override bullet prefab from the manager
    public virtual void Shoot(Vector3 aimDirection, GameObject overrideBulletPrefab = null)
    {
        if (Time.time < _lastFireTime + FireDelay || !CanShoot()) return;

        StopReload();

        if(_ammoPerShot != 0)
        {
            _currentMagAmmo--;
        }

        // Apply spray effect
        float randomAngle = UnityEngine.Random.Range(-_sprayAngle / 2f, _sprayAngle / 2f);
        aimDirection = Quaternion.Euler(0, 0, randomAngle) * aimDirection;

        Vector3 spawnPosition = shootTransform.position;
        GameObject prefabToUse = overrideBulletPrefab ?? _bulletPrefab;

        Transform bulletInstance = Instantiate(prefabToUse, spawnPosition, Quaternion.identity).transform;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (bulletInstance.TryGetComponent(out BulletBase bullet))
        {
            bullet.BulletSetup(aimDirection, angle, _bulletSpeed, _bulletDamage, _bulletKnockback, _bulletSize, _bulletLifetime, _bulletGravityScale, _hasAOE, _aoeRadius);
            bullet.PreRaycastCheck();
        }

        _pwm.SubtractAmmo(_ammoPerShot);
        _lastFireTime = Time.time;
        OnFired();
    }

    public virtual void Reload()
    {
        // Prevent reload if the magazine is full and there is no reserve ammo or if the remaining ammo exactly matches the current magazine capacity.
        if (_isReloading || _currentMagAmmo == _maxMagSize || _pwm.CurrentAmmoInventory == _currentMagAmmo) return;

        // Start the reload coroutine and store its reference
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());

        _pwm.StartReloadCoroutine();
    }

    public IEnumerator ReloadCoroutine()
    {
        _isReloading = true;

        yield return new WaitForSeconds(_reloadTime);

        int ammoNeeded = _maxMagSize - _currentMagAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, _pwm.CurrentAmmoInventory);

        _currentMagAmmo += ammoToReload;

        _isReloading = false;
        _reloadCoroutine = null;
    }

    protected void OnFired()
    {
        Fired?.Invoke();
    }

    public void StopReload()
    {
        if (_isReloading)
        {
            // Stop the reload coroutine if it is running
            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
            }

            _isReloading = false; // Reset the reloading state
        }
    }
}
