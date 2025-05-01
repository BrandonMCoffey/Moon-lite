using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Moonlite/Enemy Mineral Data")]
public class EnemyMineralData : ItemData
{
    [Header("Bullet Data")]
    [SerializeField] private EnemyBulletBase _bulletPrefab;
    [SerializeField] private TileType _tileType;
    [SerializeField] private Sprite _bulletSprite;
    [SerializeField] private Sprite _ammoSpriteBullet;
    [SerializeField] private Sprite _ammoSpriteShotgun;
    [SerializeField] private Sprite _ammoSpriteLaser;
    [SerializeField] private Sprite _ammoSpriteEffect;
    [Tooltip("Modifies the speed of the bullet from the gun by multiplying")]
    [SerializeField, Range(0.01f, 5f)] private float _bulletSpeedModifier = 1f;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _knockback = 3f;
    [SerializeField] private float _kickback = 10f;
    [SerializeField] private float _size = 0.1f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _gravityScale = 1f;
    [SerializeField] private int _bounces = 0;
    [SerializeField] private bool _hasAOE = false;
    [SerializeField, ShowIf(nameof(_hasAOE))] private float _aoeRadius = 1f;

    public EnemyBulletBase BulletPrefab => _bulletPrefab;
    public Sprite BulletSprite => _bulletSprite;
    public Sprite AmmoSpriteEffect => _ammoSpriteEffect;
    public float ShotSpeed => _bulletSpeedModifier;
    public float Damage => _damage;
    public float Knockback => _knockback;
    public float Kickback => _kickback;
    public float Size => _size;
    public float Lifetime => _lifetime;
    public float GravityScale => _gravityScale;
    public int Bounces => _bounces;
    public bool HasAOE => _hasAOE;
    public float AoeRadius => _aoeRadius;

    public TileType TileType => _tileType;
}