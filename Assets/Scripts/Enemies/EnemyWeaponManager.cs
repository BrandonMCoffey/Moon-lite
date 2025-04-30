using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;

public class EnemyWeaponManager : MonoBehaviour
{
    [Header("Weapon References")]
    [SerializeField] Transform _weaponHolder;

    [Header("Weapon Settings")]
    [SerializeField] bool _canPickupWeapons = true;
    [SerializeField, ShowIf("_canPickupWeapons")] float _pickupRadius = 1f;
    
    [Header("Weapon Inventory")]
    [SerializeField] WeaponBase _currentWeapon;
    [SerializeField] MineralData _currentAmmo;

    Coroutine _reloadCoroutine;

    float _lastFireTime = 0f;

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