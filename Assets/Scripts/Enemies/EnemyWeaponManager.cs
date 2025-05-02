using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

public class EnemyWeaponManager : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] Transform _weaponHolder;
    [SerializeField] EnemyBrain _enemyBrain;
    [SerializeField] LayerMask _bulletLayer = 1;

    // [Header("Weapon Settings")]
    // [SerializeField] bool _canPickupWeapons = true;
    // [SerializeField, ShowIf("_canPickupWeapons")] float _pickupRadius = 1f;

    [Header("Weapon Inventory")]
    [SerializeField] WeaponData _currentWeapon;
    [SerializeField] MineralData _currentAmmo;
    [SerializeField] bool _infiniteAmmo = false;

    Coroutine _reloadCoroutine;
    [SerializeField, ReadOnly] float _currentMagAmmo = 0;
    [SerializeField, ReadOnly] bool _isReloading = false;
    [SerializeField, ReadOnly] float _lastFireTime = 0f;

    private void Start()
    {
        _currentMagAmmo = _currentWeapon.MaxMagSize;
    }

    public void Attack(PlayerMovementCustom playerMovement)
    {
        if (_currentWeapon == null || _currentWeapon == null) // Check WeaponData too
        {
            Debug.LogWarning("EnemyWeaponManager: Missing current weapon or its WeaponData.");
            return;
        }
        if (_currentAmmo == null)
        {
            Debug.LogWarning("EnemyWeaponManager: Missing current ammo.");
            return;
        }
        if (playerMovement == null)
        {
            Debug.LogWarning("EnemyWeaponManager: Missing player reference.");
            return;
        }
        if (Time.timeScale == 0) return;

        HandleAiming(playerMovement);
        HandleShooting(); // You'll likely call shooting logic after aiming
    }



    public void HandleShooting()
    {
        if (_currentWeapon == null || _currentAmmo == null) return;

        Shoot();
    }

    public void HandleAiming(PlayerMovementCustom playerMovement)
    {
        Vector3 playerPos = playerMovement.transform.position;
        Vector3 playerVel = playerMovement.Velocity;
        Vector3 weaponPos = _weaponHolder.position;

        if (_currentWeapon == null)
        {
            Debug.LogError("WeaponData is null on the current weapon!");

            // Fallback: Aim directly at the player if WeaponData is missing
            Vector3 directDirection = playerPos - weaponPos;
            if (directDirection != Vector3.zero)
            {
                _weaponHolder.rotation = Quaternion.LookRotation(directDirection.normalized);
            }
            return;
        }

        float bulletSpeed = _currentWeapon.BulletSpeed * _currentAmmo.ShotSpeed;

        if (Mathf.Approximately(bulletSpeed, 0f))
        {
            Debug.LogWarning("Bullet speed is zero or negative. Cannot aim.");
            Vector3 directDirection = playerPos - weaponPos;
            if (directDirection != Vector3.zero)
            {
                _weaponHolder.rotation = Quaternion.LookRotation(directDirection.normalized);
            }
            return;
        }

        Vector3 toPlayer = playerPos - weaponPos;
        float distanceToPlayer = toPlayer.magnitude;
        float timeToReach = distanceToPlayer / bulletSpeed;
        Vector3 predictedPlayerPos = playerPos + (playerVel * timeToReach);
        Vector3 predictedDirection = predictedPlayerPos - weaponPos;

        if (predictedDirection != Vector3.zero)
        {
            // only 3D rotation
            // Quaternion targetRotation = Quaternion.LookRotation(predictedDirection.normalized);
            // float rotationSpeed = 10f;
            // _weaponHolder.rotation = Quaternion.Slerp(_weaponHolder.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // 2D rotation
            float angle = Mathf.Atan2(predictedDirection.y, predictedDirection.x) * Mathf.Rad2Deg;
            _weaponHolder.rotation = Quaternion.Euler(0f, 0f, angle + 5); // No offset needed as forward is positive x

            Vector3 localScale = transform.localScale;
            localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
            transform.localScale = localScale;

            Debug.DrawRay(weaponPos, predictedDirection.normalized * 10f, Color.red);
            Debug.DrawLine(weaponPos, predictedPlayerPos, Color.yellow);
            Debug.DrawLine(playerPos, predictedPlayerPos, Color.cyan);
        }
    }

    [Button("Test Shoot")]
    void Shoot()
    {
        var aimDirection = _weaponHolder.transform.right;
        var _data = _currentWeapon;

        if (Time.time < _lastFireTime + _currentWeapon.FireDelay) return;

        if (_currentMagAmmo <= 0 && !_infiniteAmmo)
        {
            Reload();
            return;
        }

        if (_isReloading) return;


        if (!_infiniteAmmo && _currentWeapon.AmmoPerShot > 0)
        {
            _currentMagAmmo -= _currentWeapon.AmmoPerShot;
            SubtractAmmo(_currentWeapon.AmmoPerShot);
        }

        Vector3 spawnPos = _weaponHolder.position;
        _enemyBrain.EnemyMovement.AddKnockback(-aimDirection * _data.BulletKickback * _currentAmmo.Kickback);

        Vector3 knockback = BulletSpawner.ShootBullet(_data, _currentAmmo, _bulletLayer, spawnPos, aimDirection);

        _lastFireTime = Time.time;
    }

    public void Reload()
    {
        if (_currentWeapon == null || _currentWeapon == null) return;

        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        if (_currentWeapon == null || _currentWeapon == null) yield break;


        _isReloading = true;
        yield return new WaitForSeconds(_currentWeapon.ReloadTime);

        if (_currentMagAmmo < _currentWeapon.MaxMagSize && !_infiniteAmmo)
        {
            float ammoToReload = _currentWeapon.MaxMagSize - _currentMagAmmo;
            if (ammoToReload > 0)
            {
                _currentMagAmmo += ammoToReload;
            }
        }
        else
        {
            Debug.Log("Magazine is already full or infinite ammo is enabled.");
        }

        _isReloading = false;
        _reloadCoroutine = null;
    }

    public void SubtractAmmo(float amount)
    {
        if (_currentMagAmmo > 0)
        {
            _currentMagAmmo -= amount;
        }
        else
        {
            Debug.LogWarning("No ammo left in the magazine!");
        }
    }
}
