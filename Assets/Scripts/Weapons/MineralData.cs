using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Mineral Data")]
public class MineralData : ItemData
{
    [Header("Bullet Data")]
    [SerializeField] private BulletBase _bulletPrefab;
    [SerializeField] private TileType _tileType;
    [SerializeField] private Sprite _bulletSprite;
    [SerializeField] private Sprite _ammoSpriteBullet;
    [SerializeField] private Sprite _ammoSpriteShotgun;
    [SerializeField] private Sprite _ammoSpriteLaser;
    [SerializeField] private Sprite _ammoSpriteEffect;
    [SerializeField] private SfxReference _bulletHitSfx;

    public BulletBase BulletPrefab => _bulletPrefab;
    public TileType TileType => _tileType;
    public Sprite BulletSprite => _bulletSprite;
    public Sprite AmmoSpriteEffect => _ammoSpriteEffect;

    public Sprite GetAmmoSprite(WeaponAmmoArtType type)
    {
        return type switch
        {
            WeaponAmmoArtType.Laser => _ammoSpriteLaser,
            WeaponAmmoArtType.Shotgun => _ammoSpriteShotgun,
            _ => _ammoSpriteBullet,
        };
    }

    public void PlayHitSfx()
    {
        // Use PlayAtPosition() instead?
        _bulletHitSfx.Play();
    }

    [Header("Bullet Properties")]
    [Tooltip("Modifies the speed of the bullet from the gun by multiplying")]
    [SerializeField, Range(0.01f, 5f)] private float _bulletSpeedModifier = 1f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _knockback = 3f;
    [SerializeField] private float _kickback = 10f;
    [SerializeField] private float _size = 0.1f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private int _bounces = 0;
    [SerializeField] private bool _hitOnBounce = true;
    [SerializeField] private bool _hasAOE = false;
    [SerializeField, ShowIf(nameof(_hasAOE))] private float _aoeRadius = 1f;
    [SerializeField] private float _cameraShake = 1f;

    public float ShotSpeed => _bulletSpeedModifier;
    public float Damage => _damage;
    public float Knockback => _knockback;
    public float Kickback => _kickback;
    public float Size => _size;
    public float Lifetime => _lifetime;
    public float GravityScale => _gravityScale;
    public int Bounces => _bounces;
    public bool HitOnBounce => _hitOnBounce; 
    public bool HasAOE => _hasAOE;
    public float AoeRadius => _aoeRadius;
    public float CameraShake => _cameraShake;
}

public enum WeaponAmmoArtType
{
    Default,
    Shotgun,
    Laser
}