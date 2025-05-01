using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class EnemyWeaponManager : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] Transform _weaponHolder;
    [SerializeField] EnemyBrain _enemyBrain;

    // [Header("Weapon Settings")]
    // [SerializeField] bool _canPickupWeapons = true;
    // [SerializeField, ShowIf("_canPickupWeapons")] float _pickupRadius = 1f;
    
    [Header("Weapon Inventory")]
    [SerializeField] WeaponBase _currentWeapon;
    [SerializeField] MineralData _currentAmmo;
    [SerializeField] bool _infiniteAmmo = false;

    Coroutine _reloadCoroutine;
    [SerializeField, ReadOnly] float _currentMagAmmo = 0;
    [SerializeField, ReadOnly] bool _isReloading = false;

    float _lastFireTime = 0f;

    private void Start() {
        _currentMagAmmo = _currentWeapon.Data.MaxMagSize; 
    }

    public void Attack(PlayerMovementCustom playerMovement) 
    {
        if (_currentWeapon == null || _currentWeapon.Data == null) // Check WeaponData too
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
        // HandleShooting(); // You'll likely call shooting logic after aiming
    }

    bool CanShoot()
    {
        if (_currentWeapon == null || !_currentWeapon.CanShoot()) return false;
        return true;
    }

    void Shoot(Vector3 aimDirection)
    {
        var _data = _currentWeapon.Data;

        if (Time.time < _lastFireTime + _currentWeapon.Data.FireDelay || !_currentWeapon.CanShoot()) return;

        if (_currentMagAmmo <= 0 && !_infiniteAmmo)
        {
            Reload();
            return;
        }

        if (_isReloading) return;

        _currentWeapon.StopReload();

        if (!_infiniteAmmo && _currentWeapon.Data.AmmoPerShot > 0)
        {
            _currentMagAmmo -= _currentWeapon.Data.AmmoPerShot;
            SubtractAmmo(_currentWeapon.Data.AmmoPerShot);
        }

        float randomAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
        Vector3 preSpreadAim = aimDirection;
        aimDirection = Quaternion.Euler(0, 0, randomAngle) * aimDirection;

        Vector3 spawnPos = _weaponHolder.position;
        _enemyBrain.EnemyMovement.AddKnockback(-aimDirection * _data.BulletKickback * _currentAmmo.Kickback);

        if (_data.HitScan)
        {
            Vector3 adjustedStart = spawnPos - aimDirection.normalized * 0.75f;
            float adjustedRange = _data.RayRange + 0.75f;

            for (int i = 0; i < (_data.MultipleBullets ? _data.BulletAmount : 1); i++)
            {
                // Apply spread for each raycast
                float spreadAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                WorldGenerator.Instance.DestroyTilesAlongRay(adjustedStart, spreadDirection, adjustedRange, _data.BulletDamage);

                Debug.DrawRay(adjustedStart, spreadDirection * adjustedRange, Color.red, 2f); // Visualize the ray
            }
        }
        else if (_data.MultipleBullets)
        {
            // Handle multiple bullets logic
            for (int i = 0; i < _data.BulletAmount; i++)
            {
                float spreadAngle = UnityEngine.Random.Range(-_data.SprayAngle * 0.5f, _data.SprayAngle * 0.5f);
                Vector3 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * preSpreadAim;

                BulletBase bulletInstance = Instantiate(_currentAmmo.BulletPrefab, spawnPos, Quaternion.identity);
                float angle = Mathf.Atan2(spreadDirection.y, spreadDirection.x) * Mathf.Rad2Deg;

                bulletInstance.BulletSetup(spreadDirection, angle, _data, _currentAmmo);
            }
        }
        else 
        {
            // Default single bullet logic
            BulletBase bulletInstance = Instantiate(_currentAmmo.BulletPrefab, spawnPos, Quaternion.identity);
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

            bulletInstance.BulletSetup(aimDirection, angle, _data, _currentAmmo);
        }

        _lastFireTime = Time.time;
    }

    public void Reload()
    {
        if (_currentWeapon == null || _currentWeapon.Data == null) return;
        
        if (_reloadCoroutine != null) StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        if (_currentWeapon == null || _currentWeapon.Data == null) yield break;
        

        _isReloading = true;
        yield return new WaitForSeconds(_currentWeapon.Data.ReloadTime);
        
        if (_currentMagAmmo < _currentWeapon.Data.MaxMagSize && !_infiniteAmmo)
        {
            float ammoToReload = _currentWeapon.Data.MaxMagSize - _currentMagAmmo;
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

    void SubtractAmmo(float amount)
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

    void HandleShooting()
    {
        if (_currentWeapon == null || _currentAmmo == null) return;
        
        if (CanShoot())
        {
            // TODO: Implement shooting logic here
        }
    }

    void HandleAiming(PlayerMovementCustom playerMovement)
    {
        Vector3 playerPos = playerMovement.transform.position;
        Vector3 playerVel = playerMovement.Velocity;
        Vector3 weaponPos = _weaponHolder.position;

        if (_currentWeapon.Data == null)
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

        float bulletSpeed = _currentWeapon.Data.BulletSpeed * _currentAmmo.ShotSpeed;

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
            _weaponHolder.rotation = Quaternion.Euler(0f, 0f, angle); // No offset needed as forward is positive x
        
            Vector3 localScale = transform.localScale;
            localScale.y = Mathf.Abs(localScale.y) * ((angle > 90 || angle < -90) ? -1f : 1f);
            transform.localScale = localScale;
            
            Debug.DrawRay(weaponPos, predictedDirection.normalized * 10f, Color.red);
            Debug.DrawLine(weaponPos, predictedPlayerPos, Color.yellow); 
            Debug.DrawLine(playerPos, predictedPlayerPos, Color.cyan); 
        }
    }


}