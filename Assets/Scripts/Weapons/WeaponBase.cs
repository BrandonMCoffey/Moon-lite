using UnityEngine;
using System;
using System.Collections;

public class WeaponBase : MonoBehaviour
{
    public event Action Fired = delegate { };

    [SerializeField] private WeaponData _data;
    public WeaponData Data => _data;
    [Header("Weapon Properties")]
    [SerializeField] protected string _weaponName = "Default Weapon";
    public string WeaponName => _weaponName;

    public bool IsEquipped { get; private set; } = false;
    public Transform Owner { get; private set; }
    [SerializeField] protected Transform _shootTransform;
    [SerializeField] protected Transform _gunSprite;

    protected float _currentMagAmmo; // Ammo left in the magazine.
    public float CurrentMagAmmo { get => _currentMagAmmo; set => _currentMagAmmo = value; }
    public float AmmoNeededForReload => _data.MaxMagSize - _currentMagAmmo;
    public bool IsAutomatic => _data.IsAutomatic;

    protected bool _isReloading = false;
    public bool IsReloading { get => _isReloading; set => _isReloading = value; }
    public float ReloadTime => _data.ReloadTime;

    protected float _lastFireTime;

    [Header("Ammo Consumption")]
    private float _tempAmmoPerShot = 0; // Amount of ammo consumed per shot

    protected bool _infiniteAmmo;


    protected Animator _anim;
    protected SpriteRenderer _sr;
    protected Rigidbody _rb;
    protected Collider _col;
    protected PlayerWeaponManager _pwm;
    private Coroutine _reloadCoroutine; // Track the reload coroutine

    private Coroutine _recoilCoroutine; // Track the recoil coroutine

    private void Awake()
    {
        _currentMagAmmo = _data.MaxMagSize;

        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        _anim = GetComponentInChildren<Animator>();
        _sr = GetComponentInChildren<SpriteRenderer>();

        //_sr.sprite = _data.Sprite;

        if (!_rb) Debug.LogWarning($"Rigidbody component is missing on {gameObject.name}");
        if (!_col) Debug.LogWarning($"Collider component is missing on {gameObject.name}");
        //if (!_anim) Debug.LogWarning($"Animator component is missing in children of {gameObject.name}");
        if (!_sr) Debug.LogWarning($"SpriteRenderer component is missing in children of {gameObject.name}");

        _tempAmmoPerShot = _data.AmmoPerShot;
    }

    private void Start()
    {
        _pwm = Player.WeaponManager;
    }

    private void Update()
    {
        if (PlayerInput.ToggleInfiniteAmmo)
        {
            _infiniteAmmo = !_infiniteAmmo;
            Debug.Log($"Infinite ammo cheat {(_infiniteAmmo ? "activated" : "deactivated")} for {_weaponName}!");
        }
    }

    public bool CanShoot()
    {
        return !_isReloading && (_infiniteAmmo || _currentMagAmmo > 0);
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
        var pos = PlayerInput.MousePositionWorld(transform.position.z);
        Vector3 aimDirection = (pos - transform.position).normalized;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Rotate the weapon holder to aim at the target
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip the transform vertically if aiming backward
        Vector3 localScale = transform.localScale;
        localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
        transform.localScale = localScale;
    }

    // Overload: original Shoot signature for backwards compatibility
    public virtual void Shoot(Vector3 aimDirection)
    {
        Shoot(aimDirection, null);
    }

    // Now takes an optional override bullet prefab from the manager
    public virtual void Shoot(Vector3 aimDirection, MineralData bulletData = null)
    {
        if (Time.time < _lastFireTime + _data.FireDelay || !CanShoot()) return;

        StopReload();

        if (!_infiniteAmmo && _data.AmmoPerShot > 0)
        {
            _currentMagAmmo -= _data.AmmoPerShot;
            _pwm.SubtractAmmo(_data.AmmoPerShot);
        }

        // Apply spray effect
        float randomAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
        Vector3 preSpreadAim = aimDirection;
        aimDirection = Quaternion.Euler(0, 0, randomAngle) * aimDirection;

        Vector3 spawnPosition = _shootTransform.position;

        Player.Movement.AddKnockback(-aimDirection * _data.BulletKickback * bulletData.Kickback);

        if (_data.HitScan)
        {
            // Adjust the raycast start position and range
            Vector3 adjustedStart = spawnPosition - aimDirection.normalized * .75f; // Start slightly behind
            float adjustedRange = _data.RayRange + .75f; // Extend the range to compensate

            for (int i = 0; i < (_data.MultipleBullets ? _data.BulletAmount : 1); i++)
            {
                // Apply spread for each raycast
                float spreadAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                int remainingBounces = _data.BulletBounces; // Number of allowed bounces
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
                            if (_data.ForceAoe || bulletData.HasAOE)
                            {
                                // if the weapon already has AOE, make the mult a bit smaller
                                float aoeRadius = _data.BulletAoeRadius;

                                switch(bulletData.TileType)
                                {
                                    case TileType.Explosive:
                                    if(_data.ForceAoe)
                                    {
                                        aoeRadius = _data.BulletAoeRadius * bulletData.AoeRadius * 0.5f;
                                    }
                                    else
                                    {
                                        aoeRadius = _data.BulletAoeRadius * bulletData.AoeRadius;
                                    }
                                    break;
                                }

                                chunk.World.DamageTileInCircle(pointToDestroy, aoeRadius, _data.BulletDamage * bulletData.Damage);
                            }
                            else
                            {
                                chunk.World.DamageTileAt(pointToDestroy, _data.BulletDamage * bulletData.Damage);
                            }
                        }

                        // Visualize the ray with a LineRenderer
                        CreateLaserVisual(currentStart, hit.point, _data.RayColor);

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
                        CreateLaserVisual(currentStart, currentStart + currentDirection * adjustedRange, _data.RayColor);
                        break; // Exit if no hit
                    }
                }
            }
        }
        else if (_data.MultipleBullets)
        {
            // Handle multiple bullets logic
            for (int i = 0; i < _data.BulletAmount; i++)
            {
                float spreadAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                BulletBase bulletInstance = BulletPooling.SpawnBullet(spawnPosition);
                float angle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;

                bulletInstance.BulletSetup(spreadDirection, angle, _data, bulletData);
            }
        }
        else
        {
            // Default single bullet logic
            BulletBase bulletInstance = BulletPooling.SpawnBullet(spawnPosition);
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            bulletInstance.BulletSetup(aimDirection, angle, _data, bulletData);
        }

        if (_recoilCoroutine != null)
        {
            StopCoroutine(_recoilCoroutine);
            ResetRecoil(); // Reset to original values if stopped early
        }
        _recoilCoroutine = StartCoroutine(RecoilCoroutine());

        _lastFireTime = Time.time;
        OnFired();
    }

    private void CreateLaserVisual(Vector3 start, Vector3 end, Color color)
    {
        GameObject laser = new GameObject("LaserRay");
        LineRenderer lineRenderer = laser.AddComponent<LineRenderer>();

        // Configure the LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.05f * _data.BulletSize;
        lineRenderer.endWidth = 0.05f * _data.BulletSize;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a basic material

        // Start fading the laser
        StartCoroutine(FadeLaser(laser, lineRenderer, 0.33f));
    }

    private IEnumerator FadeLaser(GameObject laser, LineRenderer lineRenderer, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = lineRenderer.startColor;
        Color endColor = new Color(0, 0, 0, 0f);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            yield return null;
        }

        Destroy(laser);
    }

    private IEnumerator RecoilCoroutine()
    {
        Transform recoilHolder = _gunSprite.parent;

        Vector3 initialPosition = recoilHolder.localPosition;
        Quaternion initialRotation = recoilHolder.localRotation;

        Vector3 targetPosition = initialPosition - new Vector3(_data.RecoilForce.x, 0f, 0f);
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0f, 0f, _data.RecoilForce.y * Mathf.Sign(_gunSprite.localScale.y));

        float elapsedTime = 0f;
        float duration = _data.FireDelay;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            recoilHolder.localPosition = Vector3.Lerp(initialPosition, targetPosition, t);
            recoilHolder.localRotation = Quaternion.Lerp(initialRotation, targetRotation, t);

            yield return null;
        }

        recoilHolder.localPosition = initialPosition;
        recoilHolder.localRotation = initialRotation;
    }

    private void ResetRecoil()
    {
        Transform recoilHolder = _gunSprite.parent;
        if (recoilHolder != null)
        {
            recoilHolder.localPosition = Vector3.zero;
            recoilHolder.localRotation = Quaternion.identity;
        }
    }

    public virtual void Reload()
    {
        // Prevent reload if the magazine is full and there is no reserve ammo or if the remaining ammo exactly matches the current magazine capacity.
        if (_isReloading || _currentMagAmmo == _data.MaxMagSize || _pwm.CurrentAmmoInventory == _currentMagAmmo) return;

        // Start the reload coroutine and store its reference
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());

        _pwm.StartReloadCoroutine();
    }

    public IEnumerator ReloadCoroutine()
    {
        _isReloading = true;

        yield return new WaitForSeconds(_data.ReloadTime);

        float ammoNeeded = _data.MaxMagSize - _currentMagAmmo;
        float ammoToReload = Mathf.Min(ammoNeeded, _pwm.CurrentAmmoInventory);

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
