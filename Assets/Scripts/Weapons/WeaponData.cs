using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string _weaponName;
    [SerializeField, TextArea] private string _weaponDescription;
    [SerializeField] private Sprite _weaponSprite;
    [SerializeField] private WeaponBase _weaponPrefab;

    public string Name => _weaponName;
    public string Description => _weaponDescription;
    public Sprite Sprite => _weaponSprite;
    public WeaponBase WeaponPrefab => _weaponPrefab;


    [Header("Reload and Ammo Properties")]
    [SerializeField] private int _maxMagSize = 10; // Ammo capacity of the magazine.
    [SerializeField] private float _reloadTime = 2f; // Time it takes to reload.
    [SerializeField] private bool _isAutomatic = false;
    [SerializeField] private int _ammoPerShot = 1; // Amount of ammo consumed per shot

    public int MaxMagSize => _maxMagSize;
    public float ReloadTime => _reloadTime;
    public bool IsAutomatic => _isAutomatic;
    public int AmmoPerShot => _ammoPerShot;


    [Header("Fire Settings")]
    [SerializeField] private float _fireRate = 4f; // Default to 4 bullets per second.
    [SerializeField] private float _bulletSpeed = 20f;
    [SerializeField] private float _sprayAngle = 0f; // Maximum spray angle in degrees
    [SerializeField] private Vector2 _recoilForce = Vector2.zero;

    public float FireRate => _fireRate;
    public float FireDelay => 1f / _fireRate;
    public float BulletSpeed => _bulletSpeed;
    public float SprayAngle => _sprayAngle;
    public Vector2 RecoilForce => _recoilForce;


    [Header("Bullet Modifiers (Multiply)")] // Multiplied on to bullet settings
    [SerializeField] private WeaponAmmoArtType _artType = WeaponAmmoArtType.Default;
    [SerializeField] private float _bulletSize = 1f;
    [SerializeField] private float _bulletDamage = 1f;
    [SerializeField] private float _bulletKnockback = 1f;
    [SerializeField] private float _bulletLifetime = 1f;
    [SerializeField] private float _bulletGravityScale = 1f;
    [SerializeField] private bool _forceAoe = false;
    [SerializeField] [ShowIf ("_forceAoe")] private float _aoeRadius = 1f;

    public WeaponAmmoArtType ArtType => _artType;
    public float BulletSize => _bulletSize;
    public float BulletDamage => _bulletDamage;
    public float BulletKnockback => _bulletKnockback;
    public float BulletLifetime => _bulletLifetime;
    public float BulletGravityScale => _bulletGravityScale;
    public bool ForceAoe => _forceAoe;
    public float BulletAoeRadius => _aoeRadius;

    [Header("Alternate Weapon Types")]
    [Tooltip("If true, make sure to change the spray angle.")]
    [SerializeField] private bool _multipleBullets = false;
    [SerializeField] [ShowIf ("_multipleBullets")] private int _bulletAmount = 1;
    [SerializeField] private bool _hitScan = false;
    [SerializeField] [ShowIf ("_hitScan")] private float _rayRange = 1f;

    public bool MultipleBullets => _multipleBullets;
    public int BulletAmount => _bulletAmount;
    public bool HitScan => _hitScan;
    public float RayRange => _rayRange;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_weaponName)) _weaponName = name;
    }
}